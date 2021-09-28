const connection = new signalR.HubConnectionBuilder()
    .withUrl("/bot")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

connection.on("OverlayReceived", (resourceUrl) => {
    const container = document.getElementById("overlayContent");

    let image = document.createElement("img");
    image.src = resourceUrl
    image.setAttribute("width", 400)

    container.appendChild(image);
});

// Start the connection.
start();