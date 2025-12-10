import requests
import json

def test_predict_endpoint():
    url = "http://localhost:8001/predict"
    payload = {
        "weekly_numbers": [5.0, 12.0, 18.0, 20.0, 33.0, 40.0]
    }
    
    try:
        response = requests.post(url, json=payload)
        print(f"Status Code: {response.status_code}")
        print(f"Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

def test_health_endpoint():
    url = "http://localhost:8001/health"
    
    try:
        response = requests.get(url)
        print(f"Health Status Code: {response.status_code}")
        print(f"Health Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Health Error: {e}")
        return False

if __name__ == "__main__":
    print("Testing FastAPI endpoints...")
    
    # Test health endpoint
    health_ok = test_health_endpoint()
    
    # Test predict endpoint
    predict_ok = test_predict_endpoint()
    
    if health_ok and predict_ok:
        print("All tests passed!")
    else:
        print("Some tests failed!")