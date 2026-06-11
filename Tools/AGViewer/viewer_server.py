from datetime import datetime
import json
from pathlib import Path
import re

import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import FileResponse, JSONResponse, PlainTextResponse, RedirectResponse
from fastapi.staticfiles import StaticFiles


app = FastAPI(title="3D AutoGain Viewer")
baseDir = Path(__file__).resolve().parent
staticDir = baseDir / "static"
configPath = baseDir / "config.json"
unityConnection: WebSocket | None = None
viewerConnections: list[WebSocket] = []
defaultConfig = {
    "maxSnapshotCount": 10,
    "latestSnapshotGraphHsv": {"h": 120, "s": 100, "v": 100},
    "oldestSnapshotGraphHsv": {"h": 0, "s": 100, "v": 100},
    "currentGainCurveGraphRgb": {"r": 38, "g": 99, "b": 180},
    "currentGainLineWidth": 3,
    "snapshotGainLineWidth": 2,
    "keepReceivedMessages": True,
}

app.mount("/static", StaticFiles(directory=staticDir), name="static")


def getTimestamp() -> str:
    return datetime.now().strftime("%Y-%m-%d %H:%M:%S")


def log(message: str) -> None:
    print(f"[{getTimestamp()}] {message}")


def mergeConfig(defaultValues: dict, overrideValues: dict) -> dict:
    mergedConfig = defaultValues.copy()

    for key, value in overrideValues.items():
        if isinstance(value, dict) and isinstance(mergedConfig.get(key), dict):
            mergedConfig[key] = {**mergedConfig[key], **value}
        else:
            mergedConfig[key] = value

    return mergedConfig


def loadConfig() -> dict:
    if not configPath.exists():
        log(f"Viewer config not found. Using default values: {configPath}")
        return defaultConfig.copy()

    try:
        with configPath.open("r", encoding="utf-8") as configFile:
            loadedConfig = json.load(configFile)
    except Exception as ex:
        log(f"Failed to load viewer config. Using default values: {ex}")
        return defaultConfig.copy()

    if not isinstance(loadedConfig, dict):
        log("Viewer config root must be an object. Using default values.")
        return defaultConfig.copy()

    return mergeConfig(defaultConfig, loadedConfig)


viewerConfig = loadConfig()


def getClientAddress(webSocket: WebSocket) -> str:
    return f"{webSocket.client.host}:{webSocket.client.port}" if webSocket.client else "unknown"


def getMessageType(message: str) -> str | None:
    match = re.search(r'"type"\s*:\s*"([^"]+)"', message)
    if match is None:
        return None

    return match.group(1)


def logUnityMessage(message: str) -> None:
    messageType = getMessageType(message)

    if messageType == "metaData":
        log("Metadata Received")
    elif messageType == "fullGainCurve":
        log("GainCurve Received")
    elif messageType == "gainSnapshot":
        log("GainSnapshot Received")
    else:
        log(f"Unity message received ({len(message)} bytes)")


async def broadcastToViewers(message: str) -> None:
    disconnectedViewers: list[WebSocket] = []

    for viewerConnection in list(viewerConnections):
        try:
            await viewerConnection.send_text(message)
        except Exception:
            disconnectedViewers.append(viewerConnection)

    for viewerConnection in disconnectedViewers:
        if viewerConnection in viewerConnections:
            viewerConnections.remove(viewerConnection)
            log("Viewer disconnected")


@app.get("/")
async def redirectToMainViewer() -> RedirectResponse:
    return RedirectResponse(url="/viewer/main")


@app.get("/viewer/main")
async def getMainViewerPage() -> FileResponse:
    return FileResponse(staticDir / "main.html")


@app.get("/viewer/log")
async def getLogViewerPage() -> FileResponse:
    return FileResponse(staticDir / "log.html")


@app.get("/config")
async def getConfig() -> JSONResponse:
    return JSONResponse(viewerConfig)


@app.get("/status", response_class=PlainTextResponse)
async def getStatus() -> str:
    connectionState = "connected" if unityConnection is not None else "disconnected"
    return (
        "3D AutoGain Viewer server is running.\n"
        f"Unity: {connectionState}\n"
        f"Viewers: {len(viewerConnections)}\n"
    )


@app.websocket("/unity")
async def handleUnityConnection(webSocket: WebSocket) -> None:
    global unityConnection

    await webSocket.accept()
    unityConnection = webSocket
    clientAddress = getClientAddress(webSocket)
    log(f"Unity connected from {clientAddress}")

    try:
        while True:
            message = await webSocket.receive_text()
            logUnityMessage(message)
            await broadcastToViewers(message)
    except WebSocketDisconnect:
        log("Unity disconnected")
    except Exception as ex:
        log(f"Unity connection error: {ex}")
    finally:
        if unityConnection is webSocket:
            unityConnection = None


@app.websocket("/viewer")
async def handleViewerConnection(webSocket: WebSocket) -> None:
    await webSocket.accept()
    viewerConnections.append(webSocket)
    clientAddress = getClientAddress(webSocket)
    log(f"Viewer connected from {clientAddress}")

    try:
        while True:
            await webSocket.receive_text()
    except WebSocketDisconnect:
        log("Viewer disconnected")
    except Exception as ex:
        log(f"Viewer connection error: {ex}")
    finally:
        if webSocket in viewerConnections:
            viewerConnections.remove(webSocket)


def main() -> None:
    uvicorn.run(app, host="127.0.0.1", port=8765)


if __name__ == "__main__":
    main()
