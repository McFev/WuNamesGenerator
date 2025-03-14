const TELEGRAM_TOKEN = '*****************tg_api_token*****************';
const NAME_URL = '*****************url__WuNamesGenerator.php';
const WEBAPP_URL = 'https://script.google.com/*****************url_self-reference*****************/exec';

function setWebhook() {
  const url = 'https://api.telegram.org/bot' + TELEGRAM_TOKEN + '/setWebhook?url=' + WEBAPP_URL;
  const response = UrlFetchApp.fetch(url);
  Logger.log(response.getContentText());
}

function doPost(e) {
  const update = JSON.parse(e.postData.contents);
  const chatId = update.message.chat.id;
  const text = update.message.text;

  if (text === "/start") {
    sendMessage(chatId, 'Now enter your full name');
  } else if (!text) {
    sendMessage(chatId, 'Responding only to text');
  }
  else {
    const url = `${NAME_URL}?name=${encodeURIComponent(text)}`;
    const response = UrlFetchApp.fetch(url);
    const obj = JSON.parse(response.getContentText());
    
    const message = '`' + obj.oldname + "` from this day forward you will also be known as\r\n`" + obj.newname + '`';
    sendMessage(chatId, message);
  }
}

function sendMessage(chatId, text) {
  const url = 'https://api.telegram.org/bot' + TELEGRAM_TOKEN + '/sendMessage';
  const payload = {
    'chat_id': chatId,
    'text': text,
    'parse_mode': 'Markdown'
  };
  const options = {
    'method': 'post',
    'contentType': 'application/json',
    'payload': JSON.stringify(payload)
  };
  UrlFetchApp.fetch(url, options);
}