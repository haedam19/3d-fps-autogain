from datetime import datetime

import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import PlainTextResponse


app = FastAPI(title="3D AutoGain Viewer")
unityConnection: WebSocket | None = None


def getTimestamp() -> str:
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")


def log(message: str) -> None:
    print(f"[{getTimestamp()}] {message}")


@app.get("/", response_class=PlainTextResponse)
async def getStatus() -> str:
    connectionState = "connected" if unityConnection is not None else "disconnected"
    return f"3D AutoGain Viewer server is running.\nUnity: {connectionState}\n"


@app.websocket("/unity")
async def handleUnityConnection(webSocket: WebSocket) -> None:
    global unityConnection

    await webSocket.accept()
    unityConnection = webSocket
    clientAddress = f"{webSocket.client.host}:{webSocket.client.port}" if webSocket.client else "unknown"
    log(f"Unity connected from {clientAddress}")

    try:
        while True:
            message = await webSocket.receive_text()
            log(f"Unity message: {message}")
    except WebSocketDisconnect:
        log("Unity disconnected")
    except Exception as ex:
        log(f"Unity connection error: {ex}")
    finally:
        if unityConnection is webSocket:
            unityConnection = None


def main() -> None:
    uvicorn.run(app, host="127.0.0.1", port=8765)


if __name__ == "__main__":
    main()
