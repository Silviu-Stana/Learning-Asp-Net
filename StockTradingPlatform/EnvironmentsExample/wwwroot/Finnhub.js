const socket = new WebSocket('wss://ws.finnhub.io?token=d462pm1r01qieo4sjog0d462pm1r01qieo4sjogg');

// Connection opened -> Subscribe
socket.addEventListener('open', function (event) {
    socket.send(JSON.stringify({ 'type': 'subscribe', 'symbol': 'MSFT' }))
});


let lastRun = 0;

// Helper: check if current time is during US market hours
function isMarketOpen() {
    const now = new Date();

    // Convert to US Eastern Time
    const utcHour = now.getUTCHours();
    const utcMinutes = now.getUTCMinutes();
    const day = now.getUTCDay(); // 0 = Sunday, 6 = Saturday

    // Market closed on weekends
    if (day === 0 || day === 6) return false;

    // Eastern Time is UTC-5 normally, UTC-4 in daylight savings
    const estOffset = -5; // standard time
    const estDate = new Date(now.getTime() + (estOffset * 60 + now.getTimezoneOffset()) * 60000);
    const hour = estDate.getHours();
    const minute = estDate.getMinutes();

    // Market hours: 9:30 to 16:00
    if (hour < 9 || hour > 16) return false;
    if (hour === 9 && minute < 30) return false;
    if (hour === 16 && minute > 0) return false;

    return true;
}

socket.addEventListener('message', function (event) {
    if (!isMarketOpen()) return; // skip if market closed

    const now = Date.now();
    if (now - lastRun < 1000) return; // only run once per second
    lastRun = now;

    try {
        const msg = JSON.parse(event.data);
        if (Array.isArray(msg.data) && msg.data.length > 0) {
            const firstP = msg.data[0].p; // get first "p"

            const priceEl = document.querySelector('#price');
            if (priceEl) {
                priceEl.textContent = firstP; // display integer
            }
        }
    } catch (err) {
        console.error('invalid JSON from websocket:', err);
    }
});

// Unsubscribe
var unsubscribe = function (symbol) {
    socket.send(JSON.stringify({ 'type': 'unsubscribe', 'symbol': symbol }));
};