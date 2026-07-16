from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.staticfiles import StaticFiles
from app.schemas import DetectionResult
from app.model import predict_defect, DETECTED_IMAGES_DIR
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("ai_service")

app = FastAPI(
    title="YOLOv8 Defect Detection API",
    description="A microservice to detect defects in images using YOLOv8.",
    version="1.0.0"
)

# Mount detected_images folder to serve annotated images statically using absolute path
app.mount("/detected_images", StaticFiles(directory=str(DETECTED_IMAGES_DIR.resolve())), name="detected_images")

@app.get("/health")
def health():
    """
    Health check endpoint to ensure service is running.
    """
    return {"status": "healthy"}

@app.post("/predict", response_model=DetectionResult)
async def predict(file: UploadFile = File(...)):
    """
    Accepts multipart image upload, runs YOLOv8 inference, and returns JSON.
    """
    # Verify it is an image upload
    if not file.content_type or not file.content_type.startswith("image/"):
        logger.warning(f"Rejected non-image upload with content-type: {file.content_type}")
        raise HTTPException(
            status_code=400,
            detail="Invalid file type. The uploaded file must be an image."
        )
        
    try:
        image_bytes = await file.read()
        if not image_bytes:
            raise HTTPException(
                status_code=400,
                detail="Empty file uploaded."
            )
            
        result = predict_defect(image_bytes, file.filename)
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error processing image {file.filename}: {e}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"An error occurred during defect detection: {str(e)}"
        )
