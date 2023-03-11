from fastapi import FastAPI
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles

app = FastAPI()

app.mount("/textures", StaticFiles(directory="textures"), name="textures")

@app.get("/")
def read_root():
    return FileResponse('index.html')

# uvicorn api:app --reload