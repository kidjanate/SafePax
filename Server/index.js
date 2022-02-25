const express = require('express');
const fs = require('fs');
const app = express();
const port = 14302;

if(!fs.existsSync("Assets")){
    console.log("Assets folder is not exists, please create it first!");
    return;
}
if(!fs.existsSync("Assets/manifest.json")){
    console.log("Manifest file is not exists, please create it first!");
    return;
}

//Setup pages
app.get('/', (req, res) => {
    res.send("<h1>You have no access to this page<h1>");
});


// Read files from manifest file
let files = fs.readFileSync("Assets/manifest.json").toString();
let json = JSON.parse(files);

app.get("/manifest", (req, res) => {
    res.send(files);
});

json.manifests.forEach(element => {
    if(!fs.existsSync("Assets/" + element.name)){
        console.log("File '" + element.name + "' is not exists, This file will not be included!");
    }else{
        app.get("/assets/" + element.name, (req, res) => {
            res.sendFile(__dirname + "/Assets/" + element.name);
        });
    }
    
});


app.listen(port, ()=>{
    console.log(`Server is running on port ${port}`);
});