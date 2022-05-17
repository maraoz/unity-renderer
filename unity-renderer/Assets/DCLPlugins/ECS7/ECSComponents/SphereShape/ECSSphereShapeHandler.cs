﻿using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSSphereShapeComponentHandler : IECSComponentHandler<ECSSphereShape>
    {
        internal AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
            DisposeMesh(scene);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSSphereShape model)
        {
            Mesh generatedMesh = null;
            if (primitiveMeshPromisePrimitive != null)
                AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);

            PrimitiveMeshModel primitiveMeshModelModel = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Sphere);
            primitiveMeshPromisePrimitive = new AssetPromise_PrimitiveMesh(primitiveMeshModelModel);
            primitiveMeshPromisePrimitive.OnSuccessEvent += shape =>
            {
                DisposeMesh(scene);
                generatedMesh = shape.mesh;
                GenerateRenderer(generatedMesh, scene, entity, model);
            };
            AssetPromiseKeeper_PrimitiveMesh.i.Keep(primitiveMeshPromisePrimitive);
        }

        private void GenerateRenderer(Mesh mesh, IParcelScene scene, IDCLEntity entity, ECSSphereShape model)
        {
            meshesInfo = ECSComponentsUtils.GenerateMeshInfo(entity, mesh, entity.gameObject, model.visible, model.withCollisions, model.isPointerBlocker);

            // Note: We should add the rendereable to the data store and dispose when it not longer exists
            rendereable = ECSComponentsUtils.AddRendereableToDataStore(scene.sceneData.id, entity.entityId, mesh, entity.gameObject, meshesInfo.renderers);
        }

        internal void DisposeMesh(IParcelScene scene)
        {
            if(meshesInfo != null)
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id,rendereable);
            
            meshesInfo = null;
            rendereable = null;
        }
    }
}