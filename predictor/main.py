from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import List
import logging
import asyncio
from blended_predictor import BlendedPredictor

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize the blended predictor
predictor = BlendedPredictor()

app = FastAPI(
    title="PredictLottoNZ Prediction Service",
    description="FastAPI service for generating lottery number predictions using ML and GPT models",
    version="1.0.0"
)

# Configure CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify actual origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models for request and response
class PredictRequest(BaseModel):
    weekly_numbers: List[float] = Field(
        ..., 
        description="Weekly lottery numbers for prediction analysis",
        json_schema_extra={"example": [5.0, 12.0, 18.0, 20.0, 33.0, 40.0]}
    )

class PredictResponse(BaseModel):
    ml_prediction: float = Field(
        ...,
        description="Machine learning model prediction",
        json_schema_extra={"example": 25.5}
    )
    gpt_prediction: float = Field(
        ...,
        description="GPT-based prediction",
        json_schema_extra={"example": 28.3}
    )
    blended_prediction: float = Field(
        ...,
        description="Blended prediction combining ML and GPT results",
        json_schema_extra={"example": 26.9}
    )

@app.get("/")
async def root():
    """Health check endpoint"""
    return {"message": "PredictLottoNZ Prediction Service is running"}

@app.get("/health")
async def health_check():
    """Health check endpoint for monitoring"""
    return {"status": "healthy", "service": "prediction-service"}

@app.post("/train")
async def train_model(historical_data: List[List[float]]):
    """
    Train the ML model with historical lottery data
    
    Args:
        historical_data: List of historical number combinations
        
    Returns:
        Training status
    """
    try:
        logger.info(f"Received training request with {len(historical_data)} historical records")
        
        # Validate historical data
        for i, combination in enumerate(historical_data):
            if len(combination) != 6:
                raise HTTPException(
                    status_code=400,
                    detail=f"Historical combination {i} must contain exactly 6 numbers"
                )
            
            for num in combination:
                if not (1 <= num <= 40):
                    raise HTTPException(
                        status_code=400,
                        detail=f"All numbers in combination {i} must be between 1 and 40"
                    )
        
        # Train the ML model
        predictor.train_ml_model(historical_data)
        
        return {
            "status": "success",
            "message": f"ML model trained with {len(historical_data)} historical combinations"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error training model: {str(e)}")
        raise HTTPException(status_code=500, detail="Internal server error during training")

@app.post("/predict", response_model=PredictResponse)
async def predict(request: PredictRequest):
    """
    Generate lottery number predictions using ML and GPT models
    
    Args:
        request: PredictRequest containing weekly numbers for analysis
        
    Returns:
        PredictResponse with ML, GPT, and blended predictions
    """
    try:
        logger.info(f"Received prediction request with weekly_numbers: {request.weekly_numbers}")
        
        # Validate input
        if len(request.weekly_numbers) != 6:
            raise HTTPException(
                status_code=400, 
                detail="weekly_numbers must contain exactly 6 numbers"
            )
        
        # Validate number range (1-40 for NZ Lotto)
        for num in request.weekly_numbers:
            if not (1 <= num <= 40):
                raise HTTPException(
                    status_code=400,
                    detail="All numbers must be between 1 and 40"
                )
        
        # Generate predictions using ML and GPT models
        ml_pred, gpt_pred, blended_pred = await predictor.predict(request.weekly_numbers)
        
        response = PredictResponse(
            ml_prediction=ml_pred,
            gpt_prediction=gpt_pred,
            blended_prediction=blended_pred
        )
        
        logger.info(f"Generated predictions: ML={ml_pred}, GPT={gpt_pred}, Blended={blended_pred}")
        return response
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error generating predictions: {str(e)}")
        raise HTTPException(status_code=500, detail="Internal server error during prediction")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)