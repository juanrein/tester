/**
 * node .
 */

const express = require('express');
const app = express();
const PORT = 8080;

let loggeri = (req, res, next) => {
    console.log(`${req.method} ${req.url} ${res.statusCode}`);
    next();
}

app.use(loggeri);
    
app.use(express.json());
 
app.get("/math/sum", (req, res) => {
    let a = parseInt(req.query.a);
    let b = parseInt(req.query.b);
    let result = a+b;
    res.status(200).send(result.toString())
})

app.get("/math/multiplication", (req, res) => {
    let a = parseInt(req.query.a);
    let b = parseInt(req.query.b);
    let result = a*b;
    res.status(200).send(result.toString())
})

app.get("/abc/upper", (req, res) => {
    let result = req.query.word.toLowerCase()
    res.status(200).send(result)
})

app.listen(
    PORT,
    () => console.log(`it's alive on http://localhost:${PORT}`)
);

