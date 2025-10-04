const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const nodemailer = require('nodemailer');
const twilio = require('twilio');
require('dotenv').config();

const app = express();
app.use(cors());
app.use(bodyParser.json());

const transporter = nodemailer.createTransport({
    service: 'SendGrid',
    auth: { user: 'apikey', pass: process.env.SENDGRID_API_KEY }
});

const twilioClient = twilio(process.env.TWILIO_ACCOUNT_SID, process.env.TWILIO_AUTH_TOKEN);

app.post('/send-email', async (req, res) => {
    const { to, subject, text } = req.body;
    try { await transporter.sendMail({ from: 'no-reply@catalog.com', to, subject, text }); res.json({ success: true }); }
    catch (err) { res.status(500).json({ error: err.message }); }
});

app.post('/send-sms', async (req, res) => {
    const { to, body } = req.body;
    try { await twilioClient.messages.create({ body, from: process.env.TWILIO_PHONE_NUMBER, to }); res.json({ success: true }); }
    catch (err) { res.status(500).json({ error: err.message }); }
});

app.listen(process.env.PORT, () => console.log(`NotificationService running on port ${process.env.PORT}`));
