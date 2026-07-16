from pydantic import BaseModel
from typing import Optional

class DetectionResult(BaseModel):
    defectType: str
    confidence: float
    isDefective: bool
    detectedImagePath: Optional[str] = None
