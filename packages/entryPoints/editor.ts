global['isEditor'] = window['isEditor'] = true

import 'engine'

import { initLocalPlayer, domReadyFuture, onWindowResize, scene } from '../engine/renderer'

import { initBabylonClient } from '../dcl'
import * as _envHelper from '../engine/renderer/envHelper'
import { canvas, engine } from '../engine/renderer/init'
import { loadedParcelSceneWorkers } from '../shared/world/parcelSceneManager'
import {
  LoadableParcelScene,
  ILandToLoadableParcelScene,
  ILand,
  IScene,
  EnvironmentData,
  ContentMapping,
  normalizeContentMappings
} from '../shared/types'
import { SceneWorker } from '../shared/world/SceneWorker'
import { WebGLParcelScene } from '../dcl/WebGLParcelScene'
import { EventEmitter } from 'events'
import { Vector3, Quaternion } from 'babylonjs'
import { SharedSceneContext } from '../engine/entities/SharedSceneContext'
import { setEditorEnvironment } from '../engine/renderer/ambientLights'
import * as Gizmos from '../engine/components/ephemeralComponents/Gizmos'
import { keyState, Keys } from '../engine/renderer/input'

let didStartPosition = false

const evtEmitter = new EventEmitter()

const CAMERA_SPEED = 0.01

const CAMERA_LEFT = Quaternion.RotationYawPitchRoll(Math.PI / 2, 0, 0)
const CAMERA_RIGHT = Quaternion.RotationYawPitchRoll(-Math.PI / 2, 0, 0)
const CAMERA_FORWARD = Quaternion.RotationYawPitchRoll(Math.PI, 0, 0)
const CAMERA_BACKWARD = Quaternion.RotationYawPitchRoll(0, 0, 0)

let cacheCheckIntervalInstance = null
let webGlParcelScene: WebGLParcelScene | null = null
let parcelsX = 1
let parcelsY = 1

async function loadScene(scene: IScene & { baseUrl: string }) {
  if (!scene) return

  let id = '0x0'
  if (scene && scene.scene && scene.scene.base) {
    const [x, y] = scene.scene.base.split(',').map($ => parseInt($, 10))
    id = `${x},${y}`
  }

  const publisher = '0x0'

  const contents = normalizeContentMappings(scene._mappings || [])

  if (!scene.baseUrl) throw new Error('baseUrl missing in scene')

  let defaultScene: ILand = {
    baseUrl: scene.baseUrl,
    scene,
    mappingsResponse: {
      contents,
      parcel_id: id,
      publisher,
      root_cid: 'Qmtest'
    }
  }

  await initializePreview(ILandToLoadableParcelScene(defaultScene), scene.scene.parcels.length)
}

async function initializePreview(userScene: EnvironmentData<LoadableParcelScene>, parcelCount: number) {
  loadedParcelSceneWorkers.forEach($ => {
    $.dispose()
    loadedParcelSceneWorkers.delete($)
  })
  webGlParcelScene = new WebGLParcelScene(userScene)
  let parcelScene = new SceneWorker(webGlParcelScene)
  const context = webGlParcelScene.context as SharedSceneContext

  context.on('uuidEvent' as any, event => {
    const { type } = event.payload

    if (type === 'gizmoSelected') {
      evtEmitter.emit('gizmoSelected', {
        gizmoType: event.payload.gizmoType,
        entityId: event.payload.entityId
      })
    } else if (type === 'gizmoDragEnded') {
      evtEmitter.emit('transform', {
        entityId: event.payload.entityId,
        transform: event.payload.transform
      })
    }
  })

  context.on('metricsUpdate', e => {
    evtEmitter.emit('metrics', {
      metrics: e.given,
      limits: e.limit
    })
  })

  // we need closeParcelScenes to enable interactions in preview mode
  loadedParcelSceneWorkers.add(parcelScene)

  if (!didStartPosition) {
    // TODO (eordano): Find a fancier way to do this
    // As the "+5,+5" is a hack to make the scene appear in front of the user
    initLocalPlayer({
      x: parcelScene.position.x + 5,
      y: 0,
      z: parcelScene.position.z - 5
    })
    didStartPosition = true
  }
}

function applyQuaternion(v: BABYLON.Vector3, q: BABYLON.Quaternion) {
  let x = v.x
  let y = v.y
  let z = v.z
  let qx = q.x
  let qy = q.y
  let qz = q.z
  let qw = q.w

  // calculate quat * vector

  let ix = qw * x + qy * z - qz * y
  let iy = qw * y + qz * x - qx * z
  let iz = qw * z + qx * y - qy * x
  let iw = -qx * x - qy * y - qz * z

  // calculate result * inverse quat

  v.x = ix * qw + iw * -qx + iy * -qz - iz * -qy
  v.y = iy * qw + iw * -qy + iz * -qx - ix * -qz
  v.z = iz * qw + iw * -qz + ix * -qy - iy * -qx

  return v
}

function moveCamera(camera: any, directionRotation: Quaternion, speed: number) {
  const direction = camera.position.subtract(camera.target).normalize()
  direction.y = 0
  applyQuaternion(direction, directionRotation)
  return direction.scaleInPlace(speed)
}

export namespace editor {
  export const babylon = BABYLON
  export let vrCamera: BABYLON.Camera | null = null
  export let arcCamera: BABYLON.ArcRotateCamera | null = null

  export async function handleMessage(message) {
    if (message.type === 'update') {
      clearInterval(cacheCheckIntervalInstance)
      await loadScene(message.payload.scene)
    }
  }

  export function getDCLCanvas() {
    return domReadyFuture.isPending ? domReadyFuture : Promise.resolve(canvas)
  }

  function configureEditorEnvironment(enabled: boolean) {
    const target = new Vector3((parcelsX * 10) / 2, 0, (parcelsY * 10) / 2)

    setEditorEnvironment(enabled)

    if (enabled) {
      getDCLCanvas()
        .then(canvas => {
          vrCamera.detachControl(canvas)
          arcCamera.attachControl(canvas, true)
        })
        .catch(() => void 0)

      arcCamera.target = target

      scene.activeCamera = arcCamera
      scene.cameraToUseForPointers = scene.activeCamera
    } else {
      getDCLCanvas()
        .then(canvas => {
          vrCamera.attachControl(canvas)
          arcCamera.detachControl(canvas)
        })
        .catch(() => void 0)

      vrCamera.position = target
      scene.activeCamera = vrCamera
      scene.cameraToUseForPointers = scene.activeCamera

      if (webGlParcelScene) {
        vrCamera.position.x = webGlParcelScene.worker.position.x + 5
        vrCamera.position.y = 1.6
        vrCamera.position.z = webGlParcelScene.worker.position.z + 5
      }
    }
  }

  /**
   * Call this function when the content mappings has changed
   */
  function setMappings(mappings: Record<string, string> | Array<ContentMapping>) {
    const context = webGlParcelScene.context as SharedSceneContext
    const seenMappings = new Set()

    const sanitizedMappings = normalizeContentMappings(mappings)

    for (let { file, hash } of sanitizedMappings) {
      seenMappings.add(file)
      context.registerMappings([{ file, hash: hash }])
    }

    context.registeredMappings.forEach((_, file) => {
      // TODO: check no textures or models or sounds are using the mappings we are removing
      if (!seenMappings.has(file)) {
        context.registeredMappings.delete(file)
      }
    })
  }

  /**
   * Call this function when wanting to send an action to the worker
   */
  export function sendExternalAction(action: { type: string; payload: { [key: string]: any } }) {
    if (webGlParcelScene && webGlParcelScene.worker) {
      const worker = webGlParcelScene.worker as SceneWorker

      if (action.payload.mappings) {
        setMappings(action.payload.mappings)
      }

      worker.engineAPI.sendSubscriptionEvent('externalAction', action)
    }
  }

  async function initializeCamera() {
    vrCamera = scene.activeCamera
    arcCamera = new BABYLON.ArcRotateCamera(
      'arc-camera',
      -Math.PI / 4,
      Math.PI / 3,
      20,
      new Vector3(5, 0, 5),
      scene,
      true
    )

    arcCamera.keysDown = []
    arcCamera.keysUp = []
    arcCamera.keysLeft = []
    arcCamera.keysRight = []

    arcCamera.onAfterCheckInputsObservable.add(() => {
      if (arcCamera === scene.activeCamera) {
        if (keyState[Keys.KEY_LEFT]) {
          arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_LEFT, CAMERA_SPEED * engine.getDeltaTime()))
        }

        if (keyState[Keys.KEY_RIGHT]) {
          arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_RIGHT, CAMERA_SPEED * engine.getDeltaTime()))
        }

        if (keyState[Keys.KEY_UP]) {
          arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_FORWARD, CAMERA_SPEED * engine.getDeltaTime()))
        }

        if (keyState[Keys.KEY_DOWN]) {
          arcCamera.target.addInPlace(moveCamera(arcCamera, CAMERA_BACKWARD, CAMERA_SPEED * engine.getDeltaTime()))
        }
      }
    })

    arcCamera.upperBetaLimit = Math.PI / 2
    arcCamera.allowUpsideDown = false
    arcCamera.panningDistanceLimit = 20
    arcCamera.pinchPrecision = 150
    arcCamera.wheelPrecision = 150
    arcCamera.lowerRadiusLimit = 10
  }

  export async function initEngine(px: number = 1, py: number = 1) {
    parcelsX = px
    parcelsY = py

    await initBabylonClient()
    await initializeCamera()
    configureEditorEnvironment(true)
    engine.setHardwareScalingLevel(0.5)
  }

  export function selectGizmo(type: Gizmos.Gizmo | null) {
    Gizmos.selectGizmo(type)
  }

  export function disableGizmo() {
    Gizmos.gizmoManager.attachToMesh(null)
  }

  export async function setPlayMode(on: boolean) {
    configureEditorEnvironment(!on)
  }

  export async function resize() {
    onWindowResize()
  }

  export function on(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.addListener(evt, listener)
  }

  export function off(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.removeListener(evt, listener)
  }

  export function setCameraZoom(zoom: number) {
    arcCamera.radius = zoom
  }

  export function setCameraRotation(alpha: number) {
    arcCamera.alpha = alpha
  }

  export const envHelper = _envHelper
}

global['editor'] = editor
