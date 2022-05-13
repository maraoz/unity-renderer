const axios = require('axios');
const inquirer = require('inquirer');
const { Console } = require('console');
const fs = require('fs');
const { Int32, ConnectionCheckOutStartedEvent } = require('mongodb');
let mongoose = require('mongoose')
let parcelSchema = new mongoose.Schema({ coords: String, id: String, dirty: Boolean })
const Parcel = mongoose.model('Parcel', parcelSchema)
const dotenv = require("dotenv")
dotenv.config()

// var logger = fs.createWriteStream('logs.txt', {flags: 'a' /*append*/})
// logger.write(`${coords}\n`)
// logger.end()

const choices = [
  'Full run', 
  'Mark all as clean',
  'Round coords for unity', 
  'Delete database'];

inquirer
  .prompt([
    {
      type: 'list',
      name: 'mode',
      message: 'What do you want to do?',
      choices: choices,
    },
  ])
  .then(answers => {
    switch (answers.mode) {
      case choices[0]:
        run().catch(error => console.log(error.stack))
        break;
      case choices[1]:
        markAllAsClean().catch(error => console.log(error.stack))
      break;
      case choices[2]:
        roundCoordsForUnity().catch(error => console.log(error.stack));
        break;
        case choices[3]:
          deleteDatabase().catch(error => console.log(error.stack))
          break;
      default:
        console.log('Option not found');
        process.exit();
    }
  });

async function run() {
  await connectToDB()

  console.log("Looking for parcels in db...")
  let dbParcels = await Parcel.find().exec()
  console.log(`Parcels found ${dbParcels.length}`)

  var allCoords = generateCoordsArray()

  // Check if need to apply filter to allCoords array
  if (dbParcels.length > 0) {
    console.log(`Filtering array (${allCoords.length}) with db parcels...`)
    allCoords = filterParcelsWithDB(allCoords, dbParcels)
    console.log(`Total parcels after filter: ${allCoords.length}`)
  }

  // Start making requests
  let result = await manageCoordsRequests(allCoords)
  console.log(result)

  // Make coords.txt in desktop for Unity
  roundCoordsForUnity()
}

// FUNCTIONS
function generateCoordsArray() {
  let maxX = 150
  let maxY = 150
  var allCoords = []
  
  for (let xIndex = -maxX; xIndex <= maxX; xIndex++) {
    for (let yIndex = -maxY; yIndex <= maxY; yIndex++) {
      allCoords.push(`${xIndex},${yIndex}`)
    }
  }
  return allCoords
}

function filterParcelsWithDB(allCoords, dbParcels) {
  let dbCoords = dbParcels.map(parcel => parcel.coords)
  return allCoords.filter(el => !dbCoords.includes(el))
}

async function manageCoordsRequests(allCoords) {
  // allCoords.length = 200 //Get N elements from array for testing purpose only
  while (allCoords.length > 0) {
    console.log(`Coords remaining: ${allCoords.length}`)
    let targetCoords = allCoords[0]
    let parcel = await getContentFromCoords(targetCoords)
    for await (const coord of parcel.pointers) {
      await compareParcelAndSave(coord, parcel.id)
    }
    allCoords = allCoords.filter(el => !parcel.pointers.includes(el))
  }
  return "FINISHED REQUESTING PARCELS"
}

async function getContentFromCoords(coords) {
  try {
    console.log(`Requesting: ${coords}`)
      const response = await axios.get(`https://peer.decentraland.org/content/entities/scene?pointer=${coords}`)
      if (response.data.length == 0) {
        console.log(`Id for ${coords}: Empty`)
        return {"id": "", "pointers": [coords]}
      }
      let parcel = response.data[0]
      if ('id' in parcel && 'pointers' in parcel) {
        console.log(`Id for ${coords}: ${parcel.id} shared with ${parcel.pointers.length - 1} other coords.`)
        return {"id": parcel.id, "pointers": parcel.pointers }
      }
      console.log(`Response without required fields: ${response}`)
  } catch(err) {
      console.log('err')
  }
}

async function compareParcelAndSave(coords, id) {
  let parcel = await Parcel.findOne({ coords: `${coords}` }).exec();
  if ((parcel != null) && (parcel.dirty == true || parcel.id == id)) {
    console.log(`Coords ${coords} already saved/dirty`)  
    return
  }
  createAndSaveParcel(coords, id)
}

// Database methods
async function connectToDB() {
  console.log("Connecting to db...")
  await mongoose.connect(`mongodb+srv://${process.env.DB_USERNAME}:${process.env.DB_PASSWORD}@mapper.odqgd.mongodb.net/myFirstDatabase?retryWrites=true&w=majority`)
  console.log("Db connected.")
}

function createAndSaveParcel(coords, id) {
  let parcel = new Parcel({ coords: coords, id: id, dirty: true })
  parcel.save()
  console.log(`Coords ${coords} saved`)
}

async function getDirtyParcels() { 
  return await Parcel.find({ dirty: true }).exec()
}

async function markAllAsClean() {
  await connectToDB()
  let parcels = await Parcel.updateMany(null, { dirty: false })
  console.log(`All cleaned - ${parcels.acknowledged} ${(await getDirtyParcels()).length}`)
}

async function deleteDatabase() {
  await connectToDB()
  await Parcel.deleteMany()
  console.log("All deleted")
}

// Generating results for Unity renderer in coords.txt
const unitySize = 5
const roundForUnitySize = x => Math.round(x/unitySize)*unitySize

async function roundCoordsForUnity() {
  console.log("Rounding coords for unity")
  await connectToDB()
  let dirtyParcels = await getDirtyParcels()
  console.log(`DirtyParcels total: ${dirtyParcels.length}`)
  let roundedCoords = []
  dirtyParcels.map(el => {
    let intCoords = el.coords.split(',')
    if (intCoords.length != 2) { return }
    let x = roundForUnitySize(parseInt(intCoords[0]))
    let y = roundForUnitySize(parseInt(intCoords[1]))
    if (Math.abs(x) > 150 || Math.abs(y) > 150) { return }
    let rounded = `${x},${y}`
    if (!roundedCoords.includes(rounded)) {
      roundedCoords.push(rounded)
    }
  })
  fs.writeFileSync("./coords.txt", roundedCoords.join("\n"))
  process.exit()
}