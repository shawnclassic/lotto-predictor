import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import StandardScaler
from typing import List
import logging

logger = logging.getLogger(__name__)

class MLPredictor:
    """Machine Learning predictor using scikit-learn regression model"""
    
    def __init__(self):
        self.model = LinearRegression()
        self.scaler = StandardScaler()
        self.is_trained = False
        
    def train_model(self, historical_data: List[List[float]]):
        """
        Train the ML model with historical lottery data
        
        Args:
            historical_data: List of historical number combinations
        """
        try:
            if len(historical_data) < 2:
                logger.warning("Insufficient historical data for training")
                return
                
            # Convert to numpy array
            data = np.array(historical_data)
            
            # Create features (previous draws) and targets (next draw average)
            X = data[:-1]  # All but last draw
            y = np.mean(data[1:], axis=1)  # Average of next draws
            
            # Scale features
            X_scaled = self.scaler.fit_transform(X)
            
            # Train model
            self.model.fit(X_scaled, y)
            self.is_trained = True
            
            logger.info(f"ML model trained with {len(X)} samples")
            
        except Exception as e:
            logger.error(f"Error training ML model: {str(e)}")
            
    def predict(self, weekly_numbers: List[float]) -> float:
        """
        Generate ML prediction based on weekly numbers
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            ML prediction value
        """
        try:
            if not self.is_trained:
                # If not trained, use a simple statistical approach
                logger.info("Model not trained, using statistical fallback")
                return self._statistical_prediction(weekly_numbers)
            
            # Scale input
            input_scaled = self.scaler.transform([weekly_numbers])
            
            # Make prediction
            prediction = self.model.predict(input_scaled)[0]
            
            logger.info(f"ML prediction generated: {prediction}")
            return float(prediction)
            
        except Exception as e:
            logger.error(f"Error generating ML prediction: {str(e)}")
            return self._statistical_prediction(weekly_numbers)
    
    def _statistical_prediction(self, weekly_numbers: List[float]) -> float:
        """
        Fallback statistical prediction when ML model is not available
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            Statistical prediction based on mean and variance
        """
        mean_val = np.mean(weekly_numbers)
        std_val = np.std(weekly_numbers)
        
        # Simple prediction: mean + small adjustment based on variance
        prediction = mean_val + (std_val * 0.1)
        
        return float(prediction)