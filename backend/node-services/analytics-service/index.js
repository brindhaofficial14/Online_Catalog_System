const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const { MongoClient } = require('mongodb');
require('dotenv').config();

const app = express();
app.use(cors());
app.use(bodyParser.json());

const client = new MongoClient(process.env.MONGO_URI);
let db;
client.connect().then(() => { db = client.db('OnlineCatalogDB'); console.log('Connected to MongoDB'); });

app.post('/log-event', async (req, res) => {
    const { type, message, user } = req.body;
    try { await db.collection('logs').insertOne({ type, message, user, timestamp: new Date() }); res.json({ success: true }); }
    catch (err) { res.status(500).json({ error: err.message }); }
});

app.get('/logs', async (req, res) => {
    try { const logs = await db.collection('logs').find().toArray(); res.json(logs); }
    catch (err) { res.status(500).json({ error: err.message }); }
});

app.listen(process.env.PORT, () => console.log(`AnalyticsService running on port ${process.env.PORT}`));
