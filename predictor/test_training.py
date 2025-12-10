import requests
import json

def test_training_endpoint():
    """Test the ML model training endpoint"""
    url = "http://localhost:8001/train"
    
    # Sample historical data (6 combinations of 6 numbers each)
    historical_data = [
        [5.0, 12.0, 18.0, 25.0, 33.0, 40.0],
        [3.0, 9.0, 16.0, 23.0, 31.0, 38.0],
        [1.0, 8.0, 15.0, 22.0, 29.0, 36.0],
        [7.0, 14.0, 21.0, 28.0, 35.0, 39.0],
        [2.0, 11.0, 19.0, 26.0, 32.0, 37.0],
        [6.0, 13.0, 20.0, 27.0, 34.0, 40.0]
    ]
    
    try:
        response = requests.post(url, json=historical_data)
        print(f"Training Status Code: {response.status_code}")
        print(f"Training Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Training Error: {e}")
        return False

def test_prediction_after_training():
    """Test prediction after training the model"""
    url = "http://localhost:8001/predict"
    payload = {
        "weekly_numbers": [4.0, 10.0, 17.0, 24.0, 30.0, 38.0]
    }
    
    try:
        response = requests.post(url, json=payload)
        print(f"Prediction Status Code: {response.status_code}")
        print(f"Prediction Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Prediction Error: {e}")
        return False

if __name__ == "__main__":
    print("Testing ML training and prediction...")
    
    # Test training
    training_ok = test_training_endpoint()
    
    # Test prediction after training
    prediction_ok = test_prediction_after_training()
    
    if training_ok and prediction_ok:
        print("All training tests passed!")
    else:
        print("Some training tests failed!")