import openai
import os
import json
import logging
from typing import List, Optional
import asyncio

logger = logging.getLogger(__name__)

class GPTPredictor:
    """GPT-based predictor using OpenAI API with few-shot prompting"""
    
    def __init__(self):
        self.client = None
        self.api_key = os.getenv('OPENAI_API_KEY')
        self.timeout = 30  # 30 second timeout
        
        if self.api_key:
            self.client = openai.OpenAI(api_key=self.api_key)
            logger.info("GPT predictor initialized with API key")
        else:
            logger.warning("No OpenAI API key found. GPT predictions will use fallback method.")
    
    async def predict(self, weekly_numbers: List[float]) -> float:
        """
        Generate GPT prediction using few-shot prompting
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            GPT prediction value
        """
        if not self.client:
            return self._fallback_prediction(weekly_numbers)
        
        try:
            # Create few-shot prompt with examples
            prompt = self._create_few_shot_prompt(weekly_numbers)
            
            # Make API call with timeout
            response = await asyncio.wait_for(
                self._make_api_call(prompt),
                timeout=self.timeout
            )
            
            # Parse response
            prediction = self._parse_response(response)
            
            logger.info(f"GPT prediction generated: {prediction}")
            return prediction
            
        except asyncio.TimeoutError:
            logger.error("GPT API call timed out")
            return self._fallback_prediction(weekly_numbers)
        except Exception as e:
            logger.error(f"Error generating GPT prediction: {str(e)}")
            return self._fallback_prediction(weekly_numbers)
    
    async def _make_api_call(self, prompt: str) -> str:
        """Make the actual OpenAI API call"""
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[
                    {
                        "role": "system",
                        "content": "You are a lottery number analysis expert. Analyze the given numbers and provide a single numerical prediction."
                    },
                    {
                        "role": "user",
                        "content": prompt
                    }
                ],
                max_tokens=100,
                temperature=0.7
            )
            
            return response.choices[0].message.content.strip()
            
        except Exception as e:
            logger.error(f"OpenAI API call failed: {str(e)}")
            raise
    
    def _create_few_shot_prompt(self, weekly_numbers: List[float]) -> str:
        """
        Create a few-shot prompt for GPT prediction
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            Formatted prompt string
        """
        prompt = """
Analyze lottery number patterns and predict the next likely average value.

Examples:
Input numbers: [5, 12, 18, 25, 33, 40]
Analysis: Mean=22.17, Range=35, Pattern shows good distribution
Prediction: 24.5

Input numbers: [1, 8, 15, 22, 29, 36]
Analysis: Mean=18.5, Range=35, Arithmetic progression pattern
Prediction: 20.2

Input numbers: [3, 9, 16, 23, 31, 38]
Analysis: Mean=20.0, Range=35, Mixed pattern with clustering
Prediction: 22.1

Now analyze these numbers:
Input numbers: {numbers}
Analysis: Provide brief analysis of mean, range, and patterns
Prediction: Provide a single numerical value between 1 and 40
""".format(numbers=weekly_numbers)
        
        return prompt
    
    def _parse_response(self, response: str) -> float:
        """
        Parse GPT response to extract numerical prediction
        
        Args:
            response: Raw GPT response
            
        Returns:
            Parsed prediction value
        """
        try:
            # Look for "Prediction:" followed by a number
            lines = response.split('\n')
            for line in lines:
                if 'Prediction:' in line or 'prediction:' in line:
                    # Extract number from the line
                    import re
                    numbers = re.findall(r'\d+\.?\d*', line)
                    if numbers:
                        prediction = float(numbers[0])
                        # Ensure prediction is within valid range
                        return max(1.0, min(40.0, prediction))
            
            # If no explicit prediction found, try to extract any number
            import re
            numbers = re.findall(r'\d+\.?\d*', response)
            if numbers:
                prediction = float(numbers[-1])  # Take the last number
                return max(1.0, min(40.0, prediction))
            
            # If no numbers found, return fallback
            logger.warning("Could not parse GPT response, using fallback")
            return 25.0  # Middle value fallback
            
        except Exception as e:
            logger.error(f"Error parsing GPT response: {str(e)}")
            return 25.0
    
    def _fallback_prediction(self, weekly_numbers: List[float]) -> float:
        """
        Fallback prediction when GPT is not available
        
        Args:
            weekly_numbers: Current week's lottery numbers
            
        Returns:
            Fallback prediction based on simple heuristics
        """
        # Simple heuristic: mean + adjustment based on spread
        mean_val = sum(weekly_numbers) / len(weekly_numbers)
        max_val = max(weekly_numbers)
        min_val = min(weekly_numbers)
        spread = max_val - min_val
        
        # Adjust prediction based on spread
        if spread > 30:  # High spread
            adjustment = 2.0
        elif spread < 15:  # Low spread
            adjustment = -1.0
        else:  # Medium spread
            adjustment = 0.5
        
        prediction = mean_val + adjustment
        
        # Ensure within valid range
        prediction = max(1.0, min(40.0, prediction))
        
        logger.info(f"GPT fallback prediction: {prediction}")
        return prediction