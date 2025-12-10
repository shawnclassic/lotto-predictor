"""
Property-based test for FastAPI integration

**Feature: predict-lotto-nz, Property 10: FastAPI integration maintains data contract**
**Validates: Requirements 6.3**
"""

import pytest
import requests
import json
from hypothesis import given, strategies as st, settings
from typing import List
import time
import subprocess
import os
import signal
import threading

class FastAPITestServer:
    """Helper class to manage FastAPI test server"""
    
    def __init__(self):
        self.process = None
        self.base_url = "http://localhost:8001"
        
    def start(self):
        """Start the FastAPI server"""
        try:
            # Start the server process
            self.process = subprocess.Popen(
                ["python", "main.py"],
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                cwd=os.path.dirname(os.path.abspath(__file__))
            )
            
            # Wait for server to start
            max_attempts = 30
            for _ in range(max_attempts):
                try:
                    response = requests.get(f"{self.base_url}/health", timeout=1)
                    if response.status_code == 200:
                        return True
                except:
                    time.sleep(1)
            
            return False
            
        except Exception as e:
            print(f"Failed to start server: {e}")
            return False
    
    def stop(self):
        """Stop the FastAPI server"""
        if self.process:
            try:
                self.process.terminate()
                self.process.wait(timeout=5)
            except:
                self.process.kill()
                self.process.wait()

# Global server instance
server = FastAPITestServer()

def setup_module():
    """Setup test module by starting the server"""
    if not server.start():
        pytest.skip("Could not start FastAPI server")

def teardown_module():
    """Teardown test module by stopping the server"""
    server.stop()

# Strategy for generating valid lottery numbers (1-40)
valid_lottery_number = st.floats(min_value=1.0, max_value=40.0)

# Strategy for generating valid weekly numbers (exactly 6 numbers)
valid_weekly_numbers = st.lists(
    valid_lottery_number,
    min_size=6,
    max_size=6
)

# Strategy for generating invalid weekly numbers (wrong size)
invalid_size_weekly_numbers = st.one_of(
    st.lists(valid_lottery_number, min_size=0, max_size=5),  # Too few
    st.lists(valid_lottery_number, min_size=7, max_size=10)  # Too many
)

# Strategy for generating out-of-range numbers
invalid_range_numbers = st.lists(
    st.one_of(
        st.floats(min_value=-100.0, max_value=0.9),  # Below range
        st.floats(min_value=40.1, max_value=100.0)   # Above range
    ),
    min_size=6,
    max_size=6
)

@given(weekly_numbers=valid_weekly_numbers)
@settings(max_examples=100, deadline=10000)  # Run 100 iterations with 10s timeout
def test_predict_endpoint_data_contract(weekly_numbers: List[float]):
    """
    Property: FastAPI integration maintains data contract
    
    For any valid weekly numbers (6 floats between 1-40), the predict endpoint should:
    1. Return HTTP 200 status
    2. Return response with ml_prediction, gpt_prediction, and blended_prediction fields
    3. All prediction values should be floats
    4. All prediction values should be within reasonable range (1-40)
    """
    try:
        # Make prediction request
        response = requests.post(
            f"{server.base_url}/predict",
            json={"weekly_numbers": weekly_numbers},
            timeout=30  # Allow time for GPT calls
        )
        
        # Verify HTTP status
        assert response.status_code == 200, f"Expected 200, got {response.status_code}"
        
        # Parse response
        data = response.json()
        
        # Verify required fields exist
        required_fields = ["ml_prediction", "gpt_prediction", "blended_prediction"]
        for field in required_fields:
            assert field in data, f"Missing required field: {field}"
        
        # Verify field types
        for field in required_fields:
            assert isinstance(data[field], (int, float)), f"Field {field} should be numeric, got {type(data[field])}"
        
        # Verify prediction values are within reasonable range
        for field in required_fields:
            value = data[field]
            assert 1.0 <= value <= 40.0, f"Field {field} value {value} is outside valid range [1, 40]"
        
        # Verify blended prediction is reasonable relative to ML and GPT
        ml_pred = data["ml_prediction"]
        gpt_pred = data["gpt_prediction"]
        blended_pred = data["blended_prediction"]
        
        # Blended should be between ML and GPT predictions (or close to them)
        min_pred = min(ml_pred, gpt_pred)
        max_pred = max(ml_pred, gpt_pred)
        
        # Allow some tolerance for blending algorithm
        tolerance = abs(max_pred - min_pred) * 0.5 + 1.0
        assert min_pred - tolerance <= blended_pred <= max_pred + tolerance, \
            f"Blended prediction {blended_pred} is not reasonable given ML {ml_pred} and GPT {gpt_pred}"
        
    except requests.exceptions.Timeout:
        pytest.fail("Request timed out - service may be overloaded")
    except requests.exceptions.RequestException as e:
        pytest.fail(f"Request failed: {e}")

@given(weekly_numbers=invalid_size_weekly_numbers)
@settings(max_examples=50)
def test_predict_endpoint_rejects_invalid_size(weekly_numbers: List[float]):
    """
    Property: FastAPI integration rejects invalid input sizes
    
    For any weekly numbers list that doesn't contain exactly 6 numbers,
    the predict endpoint should return HTTP 400 error.
    """
    try:
        response = requests.post(
            f"{server.base_url}/predict",
            json={"weekly_numbers": weekly_numbers},
            timeout=5
        )
        
        # Should return 400 for invalid size
        assert response.status_code == 400, f"Expected 400 for invalid size, got {response.status_code}"
        
        # Response should contain error details
        data = response.json()
        assert "detail" in data, "Error response should contain 'detail' field"
        
    except requests.exceptions.RequestException as e:
        pytest.fail(f"Request failed: {e}")

@given(weekly_numbers=invalid_range_numbers)
@settings(max_examples=50)
def test_predict_endpoint_rejects_invalid_range(weekly_numbers: List[float]):
    """
    Property: FastAPI integration rejects out-of-range numbers
    
    For any weekly numbers containing values outside [1, 40] range,
    the predict endpoint should return HTTP 400 error.
    """
    try:
        response = requests.post(
            f"{server.base_url}/predict",
            json={"weekly_numbers": weekly_numbers},
            timeout=5
        )
        
        # Should return 400 for invalid range
        assert response.status_code == 400, f"Expected 400 for invalid range, got {response.status_code}"
        
        # Response should contain error details
        data = response.json()
        assert "detail" in data, "Error response should contain 'detail' field"
        
    except requests.exceptions.RequestException as e:
        pytest.fail(f"Request failed: {e}")

def test_health_endpoint_always_works():
    """
    Simple test to verify health endpoint works consistently
    """
    response = requests.get(f"{server.base_url}/health", timeout=5)
    assert response.status_code == 200
    
    data = response.json()
    assert data["status"] == "healthy"
    assert data["service"] == "prediction-service"

if __name__ == "__main__":
    # Run the tests
    pytest.main([__file__, "-v"])