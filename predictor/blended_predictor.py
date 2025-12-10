import logging
from typing import List, Tuple
from ml_predictor import MLPredictor
from gpt_predictor import GPTPredictor

logger = logging.getLogger(__name__)

class BlendedPredictor:
    """Blended predictor that combines ML and GPT predictions"""
    
    def __init__(self):
        self.ml_predictor = MLPredictor()
        self.gpt_predictor = GPTPredictor()
        
        # Weights for blending (can be adjusted based on performance)
        self.ml_weight = 0.6
        self.gpt_weight = 0.4
        
    def train_ml_model(self, historical_data: List[List[float]]):
        """
        Train the ML component with historical data
        
        Args:
            historical_data: List of historical number combinations
        """
        self.ml_predictor.train_model(historical_data)
        
    async def predict(self, weekly_numbers: List[float]) -> Tuple[float, float, float]:
        """
        Generate blended prediction combining ML and GPT results
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            Tuple of (ml_prediction, gpt_prediction, blended_prediction)
        """
        try:
            # Get ML prediction
            ml_prediction = self.ml_predictor.predict(weekly_numbers)
            
            # Get GPT prediction
            gpt_prediction = await self.gpt_predictor.predict(weekly_numbers)
            
            # Calculate blended prediction
            blended_prediction = self._blend_predictions(ml_prediction, gpt_prediction)
            
            logger.info(f"Predictions - ML: {ml_prediction}, GPT: {gpt_prediction}, Blended: {blended_prediction}")
            
            return ml_prediction, gpt_prediction, blended_prediction
            
        except Exception as e:
            logger.error(f"Error generating blended prediction: {str(e)}")
            # Return fallback predictions
            fallback = sum(weekly_numbers) / len(weekly_numbers)
            return fallback, fallback * 1.1, fallback * 1.05
    
    def _blend_predictions(self, ml_pred: float, gpt_pred: float) -> float:
        """
        Blend ML and GPT predictions using weighted average
        
        Args:
            ml_pred: ML prediction value
            gpt_pred: GPT prediction value
            
        Returns:
            Blended prediction value
        """
        try:
            # Weighted average
            blended = (ml_pred * self.ml_weight) + (gpt_pred * self.gpt_weight)
            
            # Ensure within valid range
            blended = max(1.0, min(40.0, blended))
            
            return blended
            
        except Exception as e:
            logger.error(f"Error blending predictions: {str(e)}")
            # Return simple average as fallback
            return (ml_pred + gpt_pred) / 2
    
    def update_weights(self, ml_weight: float, gpt_weight: float):
        """
        Update blending weights
        
        Args:
            ml_weight: Weight for ML prediction
            gpt_weight: Weight for GPT prediction
        """
        # Normalize weights to sum to 1
        total = ml_weight + gpt_weight
        if total > 0:
            self.ml_weight = ml_weight / total
            self.gpt_weight = gpt_weight / total
            logger.info(f"Updated weights - ML: {self.ml_weight}, GPT: {self.gpt_weight}")
        else:
            logger.warning("Invalid weights provided, keeping current weights")