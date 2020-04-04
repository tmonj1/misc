const mailer = require('nodemailer');

// mailcatcher settings
const mailcatcher = {
  host: "localhost",
  port: 1025,
  secure: false
};

// mail message
const message = {
  from: 'taro@example.com',
  to: 'you@example.com',
  subject: 'テスト',
  text: 'テストメール',
  html: '<b>テストメール</b>'
};

// send a message
const transporter = mailer.createTransport(mailcatcher);
transporter.sendMail(message);
