# Advanced LLM Setup and Configuration Guide

This comprehensive guide covers the setup, configuration, and management of advanced Large Language Model (LLM) features in PredictLottoNZ.

## Table of Contents

- [Overview](#overview)
- [Amazon Bedrock AgentCore Setup](#amazon-bedrock-agentcore-setup)
- [Local LLM Service Deployment](#local-llm-service-deployment)
- [Accuracy Analysis Configuration](#accuracy-analysis-configuration)
- [Model Retraining Pipeline](#model-retraining-pipeline)
- [Confidence Scoring and Reasoning](#confidence-scoring-and-reasoning)
- [Monitoring and Troubleshooting](#monitoring-and-troubleshooting)

## Overview

The Advanced LLM system provides enterprise-grade AI capabilities with:

- **Multi-Provider Architecture**: Amazon Bedrock, Local LLM, FastAPI, and Frequency-based providers
- **Intelligent Fallback Chain**: Automatic provider switching based on health and performance
- **Continuous Learning**: Automated model retraining based on accuracy analysis
- **Zero-Downtime Updates**: Hot-swapping capabilities for seamless model updates
- **Comprehensive Monitoring**: Real-time performance tracking and alerting

## Amazon Bedrock AgentCore Setup

### Prerequisites

- AWS Account with Bedrock access
- IAM permissions for model invocation and training
- VPC configuration (optional, for enhanced security)
- Valid payment method for AWS services

### Step 1: AWS Account Configuration

#### Create IAM Policy
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "bedrock:InvokeModel",
        "bedrock:InvokeModelWithResponseStream",
        "bedrock:ListFoundationModels",
        "bedrock:GetFoundationModel",
        "bedrock:CreateModelCustomizationJob",
        "bedrock:GetModelCustomizationJob",
        "bedrock:ListModelCustomizationJobs",
        "bedrock:StopModelCustomizationJob"
      ],
      "Resource": [
        "arn:aws:bedrock:*::foundation-model/anthropic.claude-3-sonnet-20240229-v1:0",
        "arn:aws:bedrock:*::foundation-model/anthropic.claude-3-haiku-20240307-v1:0",
        "arn:aws:bedrock:*:*:model-customization-job/*"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:log-group:/aws/bedrock/*"
    }
  ]
}
```
#### Create IAM Role
```bash
# Create IAM role for Bedrock access
aws iam create-role \
  --role-name PredictLottoBedrockRole \
  --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [
      {
        "Effect": "Allow",
        "Principal": {
          "Service": "ec2.amazonaws.com"
        },
        "Action": "sts:AssumeRole"
      }
    ]
  }'

# Attach the policy to the role
aws iam attach-role-policy \
  --role-name PredictLottoBedrockRole \
  --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/PredictLottoBedrockPolicy

# Create instance profile
aws iam create-instance-profile \
  --instance-profile-name PredictLottoBedrockProfile

# Add role to instance profile
aws iam add-role-to-instance-profile \
  --instance-profile-name PredictLottoBedrockProfile \
  --role-name PredictLottoBedrockRole
```

### Step 2: Model Access Request

#### Request Model Access via AWS Console
1. Navigate to AWS Console > Amazon Bedrock > Model access
2. Request access to the following models:
   - **Anthropic Claude 3 Sonnet** (anthropic.claude-3-sonnet-20240229-v1:0)
   - **Anthropic Claude 3 Haiku** (anthropic.claude-3-haiku-20240307-v1:0) [Optional]
3. Wait for approval (typically 5-10 minutes)

#### Verify Model Access
```bash
# List available models
aws bedrock list-foundation-models --region us-west-2

# Test specific model access
aws bedrock get-foundation-model \
  --model-identifier anthropic.claude-3-sonnet-20240229-v1:0 \
  --region us-west-2
```

### Step 3: Environment Configuration

#### Production Environment Variables
```env
# AWS Authentication
AWS_REGION=us-west-2
AWS_ACCESS_KEY_ID=your_access_key_id
AWS_SECRET_ACCESS_KEY=your_secret_access_key

# Bedrock Model Configuration
BEDROCK_MODEL_ID=anthropic.claude-3-sonnet-20240229-v1:0
BEDROCK_FALLBACK_MODEL_ID=anthropic.claude-3-haiku-20240307-v1:0
BEDROCK_TIMEOUT_SECONDS=30
BEDROCK_MAX_RETRIES=3
BEDROCK_RETRY_DELAY_MS=1000

# Advanced Bedrock Settings
BEDROCK_MAX_TOKENS=4096
BEDROCK_TEMPERATURE=0.7
BEDROCK_TOP_P=0.9
BEDROCK_STOP_SEQUENCES=["Human:", "Assistant:"]

# Performance and Cost Optimization
BEDROCK_ENABLE_STREAMING=true
BEDROCK_BATCH_SIZE=1
BEDROCK_COST_THRESHOLD_USD=100.00
BEDROCK_ENABLE_CACHING=true
BEDROCK_CACHE_TTL_MINUTES=60

# Security and Compliance
BEDROCK_ENABLE_LOGGING=true
BEDROCK_LOG_LEVEL=INFO
BEDROCK_ENCRYPT_PAYLOADS=true
BEDROCK_VPC_ENDPOINT_ID=vpce-12345678  # Optional
```

### Step 4: Testing Bedrock Integration

#### Basic Connectivity Test
```bash
# Test Bedrock service health
curl -X GET "http://localhost:5000/api/predictions/providers/health" | jq '.providers.BedrockAgentCore'

# Generate test prediction
curl -X GET "http://localhost:5000/api/predictions/enhanced?count=1&provider=BedrockAgentCore&includeReasoning=true"

# Check Bedrock usage metrics
aws bedrock get-model-invocation-logging-configuration --region us-west-2
```

#### Cost Monitoring Setup
```bash
# Set up CloudWatch billing alerts
aws cloudwatch put-metric-alarm \
  --alarm-name "BedrockCostAlert" \
  --alarm-description "Alert when Bedrock costs exceed threshold" \
  --metric-name EstimatedCharges \
  --namespace AWS/Billing \
  --statistic Maximum \
  --period 86400 \
  --threshold 100 \
  --comparison-operator GreaterThanThreshold \
  --dimensions Name=Currency,Value=USD Name=ServiceName,Value=AmazonBedrock \
  --evaluation-periods 1
```

## Local LLM Service Deployment

### Prerequisites

- Python 3.11+ with pip
- 8GB+ RAM (16GB+ recommended)
- Optional: NVIDIA GPU with CUDA 11.8+ for acceleration
- Docker and Docker Compose

### Step 1: Model Preparation

#### Directory Structure Setup
```bash
# Create comprehensive model directory structure
mkdir -p models/{base-models,fine-tuned,checkpoints,configs,archives}

# Expected structure:
models/
├── base-models/                    # Foundation models
│   ├── llama-2-7b-chat/
│   ├── mistral-7b-instruct/
│   └── codellama-7b-instruct/
├── fine-tuned/                     # Custom lottery prediction models
│   ├── lotto-predictor-v1.0/
│   ├── lotto-predictor-v2.0/
│   └── lotto-predictor-v2.1/
├── checkpoints/                    # Training checkpoints
│   └── training-runs/
├── configs/                        # Model configurations
│   ├── training-configs/
│   └── inference-configs/
└── archives/                       # Archived models
    └── deprecated/
```

#### Model File Requirements
Each model directory must contain:
```bash
models/fine-tuned/lotto-predictor-v2.1/
├── model.safetensors              # Model weights (SafeTensors format)
├── config.json                    # Model configuration
├── tokenizer.json                 # Tokenizer configuration
├── tokenizer_config.json          # Tokenizer settings
├── special_tokens_map.json        # Special tokens mapping
├── generation_config.json         # Generation parameters
├── model_metadata.json            # Custom metadata
├── training_log.txt               # Training history
└── README.md                      # Model documentation
```

#### Download Base Models
```bash
# Option 1: Download from Hugging Face
pip install huggingface_hub
python -c "
from huggingface_hub import snapshot_download
snapshot_download(
    repo_id='microsoft/DialoGPT-medium',
    local_dir='./models/base-models/dialogpt-medium',
    local_dir_use_symlinks=False
)
"

# Option 2: Use our pre-trained lottery model (if available)
wget https://releases.predict-lotto-nz.com/models/lotto-predictor-v2.1.tar.gz
tar -xzf lotto-predictor-v2.1.tar.gz -C models/fine-tuned/

# Option 3: Train custom model (see training section)
```

### Step 2: Hardware Configuration

#### CPU-Only Setup
```env
# Basic CPU configuration
LOCAL_LLM_GPU_ENABLED=false
LOCAL_LLM_CPU_THREADS=8
LOCAL_LLM_MAX_MEMORY_GB=8
LOCAL_LLM_QUANTIZATION=int8  # Reduce memory usage
LOCAL_LLM_BATCH_SIZE=1
```

#### GPU Setup (Recommended)
```bash
# Install NVIDIA drivers and CUDA
sudo apt update
sudo apt install -y nvidia-driver-525 nvidia-cuda-toolkit

# Install NVIDIA Container Toolkit
distribution=$(. /etc/os-release;echo $ID$VERSION_ID)
curl -s -L https://nvidia.github.io/nvidia-docker/gpgkey | sudo apt-key add -
curl -s -L https://nvidia.github.io/nvidia-docker/$distribution/nvidia-docker.list | sudo tee /etc/apt/sources.list.d/nvidia-docker.list

sudo apt-get update && sudo apt-get install -y nvidia-docker2
sudo systemctl restart docker

# Install PyTorch with CUDA support
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# GPU configuration
LOCAL_LLM_GPU_ENABLED=true
LOCAL_LLM_GPU_DEVICE=0
LOCAL_LLM_GPU_MEMORY_FRACTION=0.8
LOCAL_LLM_QUANTIZATION=fp16
LOCAL_LLM_BATCH_SIZE=4
```

### Step 3: Service Configuration

#### Production Environment Variables
```env
# Service Settings
LOCAL_LLM_BASE_URL=http://localhost:8002
LOCAL_LLM_SERVICE_PORT=8002
LOCAL_LLM_WORKERS=1
LOCAL_LLM_TIMEOUT_SECONDS=60

# Model Configuration
LOCAL_LLM_MODEL_PATH=/models/fine-tuned/lotto-predictor-v2.1
LOCAL_LLM_MODEL_NAME=lotto-predictor-v2.1
LOCAL_LLM_FALLBACK_MODEL_PATH=/models/fine-tuned/lotto-predictor-v2.0
LOCAL_LLM_AUTO_MODEL_SELECTION=true

# Performance Settings
LOCAL_LLM_MAX_SEQUENCE_LENGTH=2048
LOCAL_LLM_ENABLE_CACHING=true
LOCAL_LLM_CACHE_SIZE_MB=1024

# Generation Parameters
LOCAL_LLM_TEMPERATURE=0.7
LOCAL_LLM_TOP_P=0.9
LOCAL_LLM_TOP_K=50
LOCAL_LLM_MAX_NEW_TOKENS=512
LOCAL_LLM_REPETITION_PENALTY=1.1

# Hot-Swapping Configuration
LOCAL_LLM_ENABLE_HOT_SWAP=true
LOCAL_LLM_SWAP_VALIDATION_SAMPLES=5
LOCAL_LLM_SWAP_TIMEOUT_MINUTES=10
LOCAL_LLM_ENABLE_ROLLBACK=true
LOCAL_LLM_KEEP_OLD_MODEL_MINUTES=30

# Monitoring and Logging
LOCAL_LLM_LOG_LEVEL=INFO
LOCAL_LLM_ENABLE_METRICS=true
LOCAL_LLM_METRICS_PORT=8003
LOCAL_LLM_HEALTH_CHECK_INTERVAL=30
```

### Step 4: Docker Deployment

#### Production Docker Configuration
```yaml
# docker-compose.llm.yml
version: '3.8'
services:
  local-llm:
    build:
      context: ./predictor
      dockerfile: Dockerfile.llm
    ports:
      - "${LOCAL_LLM_SERVICE_PORT:-8002}:8002"
      - "${LOCAL_LLM_METRICS_PORT:-8003}:8003"
    volumes:
      - ./models:/models:ro
      - ./predictor/training:/training
      - llm-cache:/cache
      - llm-logs:/logs
    environment:
      - LOCAL_LLM_GPU_ENABLED=${LOCAL_LLM_GPU_ENABLED:-false}
      - LOCAL_LLM_MODEL_PATH=${LOCAL_LLM_MODEL_PATH}
      - LOCAL_LLM_MAX_MEMORY_GB=${LOCAL_LLM_MAX_MEMORY_GB:-8}
    deploy:
      resources:
        limits:
          memory: 16G
        reservations:
          memory: 8G
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8002/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 120s

volumes:
  llm-cache:
    driver: local
  llm-logs:
    driver: local
```

### Step 5: Model Training and Fine-tuning

#### Training Data Preparation
```bash
# Export training data from the system
curl -X GET "http://localhost:5000/api/training/data/summary"
curl -X GET "http://localhost:5000/api/training-data/export?format=llm" > training_data.jsonl

# Validate training data format
python validate_training_data.py --input training_data.jsonl
```

#### Fine-tuning Process
```bash
# Start fine-tuning process
python predictor/local_llm_retraining.py \
  --base_model models/base-models/llama-2-7b-chat \
  --training_data training_data.jsonl \
  --output_dir models/fine-tuned/lotto-predictor-v2.2 \
  --epochs 10 \
  --learning_rate 2e-5 \
  --batch_size 4 \
  --gradient_accumulation_steps 4 \
  --lora_rank 16 \
  --lora_alpha 32 \
  --lora_dropout 0.1 \
  --warmup_steps 100 \
  --save_steps 500 \
  --eval_steps 100 \
  --logging_steps 10

# Monitor training progress
tail -f models/fine-tuned/lotto-predictor-v2.2/training.log
```

### Step 6: Service Deployment and Testing

#### Start Local LLM Service
```bash
# Using Docker Compose (Recommended)
docker-compose -f docker-compose.yml -f docker-compose.llm.yml up -d local-llm

# Or run directly with Python
cd predictor
python local_llm_service.py

# Check service health
curl http://localhost:8002/health

# View model information
curl http://localhost:8002/model/info | jq
```

#### Integration Testing
```bash
# Test prediction generation
curl -X POST http://localhost:8002/predict \
  -H "Content-Type: application/json" \
  -d '{
    "historical_data": [
      {"draw": 1999, "numbers": [7, 14, 21, 28, 35, 42], "date": "2024-12-07"}
    ],
    "prediction_count": 1
  }' | jq

# Test through main API
curl -X GET "http://localhost:5000/api/predictions/enhanced?count=1&provider=LocalLLM&includeReasoning=true"

# Check provider health
curl -X GET "http://localhost:5000/api/predictions/providers/health" | jq '.providers.LocalLLM'
```

## Accuracy Analysis Configuration

### Automated Analysis Setup

#### Configuration Parameters
```env
# Accuracy Analysis Settings
ACCURACY_ANALYSIS_ENABLED=true
ACCURACY_ANALYSIS_SCHEDULE_CRON="0 */6 * * *"  # Every 6 hours
ACCURACY_ANALYSIS_MIN_PREDICTIONS=10
ACCURACY_ANALYSIS_LOOKBACK_DAYS=30
ACCURACY_ANALYSIS_STATISTICAL_SIGNIFICANCE=0.05

# Threshold Configuration
ACCURACY_THRESHOLD_WARNING=0.12
ACCURACY_THRESHOLD_CRITICAL=0.08
CONFIDENCE_CALIBRATION_THRESHOLD=0.15
PERFORMANCE_DEGRADATION_THRESHOLD=0.05

# Notification Settings
ACCURACY_WEBHOOK_URL=https://hooks.slack.com/your-webhook
ACCURACY_EMAIL_NOTIFICATIONS=admin@your-domain.com
ACCURACY_ALERT_COOLDOWN_MINUTES=60
```

#### Analysis Triggers
```bash
# Automatic triggers
1. New lottery draw imported → Immediate analysis
2. Scheduled analysis → Every 6 hours
3. Provider performance drop → Real-time monitoring
4. Manual trigger → API endpoint

# Manual analysis trigger
curl -X POST http://localhost:5000/api/accuracy/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "trigger": "manual",
    "analysisType": "comprehensive",
    "dateRange": {
      "from": "2024-01-01T00:00:00Z",
      "to": "2024-12-12T23:59:59Z"
    },
    "providers": ["all"],
    "includeStatisticalTests": true,
    "generateRecommendations": true
  }'
```

### Metrics and Reporting

#### Key Performance Indicators
```bash
# Accuracy Metrics
- Exact Match Rate: Percentage of predictions with all 6 numbers correct
- Partial Match Rate: Percentage with 3+ numbers correct
- Proximity Score: Average distance to winning numbers
- Confidence Calibration: Correlation between confidence and accuracy

# Provider Metrics
- Success Rate: Percentage of successful API calls
- Response Time: Average response time in milliseconds
- Cost Efficiency: Accuracy per dollar spent
- Reliability Score: Uptime and consistency metrics
```

#### Dashboard Configuration
```bash
# Access accuracy dashboard
http://localhost:3000/accuracy-dashboard

# Key dashboard features:
- Real-time accuracy trends
- Provider performance comparison
- Confidence score distribution
- Statistical significance indicators
- Automated recommendations
- Cost-effectiveness analysis
```

## Model Retraining Pipeline

### Pipeline Configuration

#### Retraining Triggers
```env
# Trigger Configuration
RETRAINING_ENABLED=true
RETRAINING_ACCURACY_THRESHOLD=0.85  # Trigger if accuracy drops below 85% of baseline
RETRAINING_CONFIDENCE_DRIFT_THRESHOLD=0.1
RETRAINING_MIN_DATA_POINTS=50
RETRAINING_SCHEDULE_CRON="0 2 * * 0"  # Weekly on Sunday at 2 AM
RETRAINING_MAX_MODEL_AGE_DAYS=90
RETRAINING_STATISTICAL_SIGNIFICANCE=0.05
RETRAINING_PARALLEL_PROVIDERS=true
RETRAINING_AUTO_DEPLOY=false  # Require manual approval
```

#### Training Parameters
```env
# Training Configuration
TRAINING_GPU_ENABLED=true
TRAINING_MAX_EPOCHS=20
TRAINING_EARLY_STOPPING_PATIENCE=5
TRAINING_LEARNING_RATE=2e-5
TRAINING_BATCH_SIZE=4
TRAINING_GRADIENT_ACCUMULATION_STEPS=4
TRAINING_WARMUP_RATIO=0.1
TRAINING_WEIGHT_DECAY=0.01
TRAINING_MAX_GRAD_NORM=1.0

# LoRA Configuration (for efficient fine-tuning)
TRAINING_USE_LORA=true
TRAINING_LORA_RANK=16
TRAINING_LORA_ALPHA=32
TRAINING_LORA_DROPOUT=0.1
TRAINING_LORA_TARGET_MODULES=["q_proj", "v_proj", "k_proj", "o_proj"]
```

### Pipeline Execution

#### Manual Retraining Trigger
```bash
curl -X POST http://localhost:5000/api/training/retrain \
  -H "Content-Type: application/json" \
  -d '{
    "trigger": "manual",
    "providers": ["BedrockAgentCore", "LocalLLM"],
    "trainingParameters": {
      "epochs": 10,
      "learningRate": 0.001,
      "batchSize": 32,
      "validationSplit": 0.2,
      "enableEarlyStopping": true,
      "patienceEpochs": 3
    },
    "dataFilters": {
      "fromDate": "2024-01-01T00:00:00Z",
      "minAccuracy": 0.1,
      "includeProviders": ["all"],
      "excludeOutliers": true
    },
    "deploymentStrategy": {
      "autoDeployOnSuccess": false,
      "requireManualApproval": true,
      "enableABTesting": true,
      "rollbackOnFailure": true
    }
  }'
```

#### Monitor Training Progress
```bash
# Check training status
curl -X GET "http://localhost:5000/api/training/status" | jq

# View specific job details
curl -X GET "http://localhost:5000/api/training/jobs/retrain_local_20241212_103000" | jq

# Monitor training logs
curl -X GET "http://localhost:5000/api/training/logs/retrain_local_20241212_103000?tail=100"
```

### Model Deployment

#### Blue-Green Deployment Process
```bash
# 1. Validation Phase
- Deploy new model to staging environment
- Run comprehensive validation tests
- Compare performance against production model
- Generate deployment recommendation

# 2. Gradual Rollout
- Deploy to 10% of traffic (canary)
- Monitor for 24 hours
- Gradually increase: 10% → 25% → 50% → 100%
- Automatic rollback if performance degrades

# 3. Full Deployment
- Complete traffic switch
- Archive previous model
- Update documentation
- Send notifications
```

#### Rollback Procedures
```bash
# Automatic rollback triggers
1. Accuracy drops below 90% of previous performance
2. Response time increases by >50%
3. Error rate exceeds 5%
4. Confidence calibration error >0.15

# Manual rollback
curl -X POST http://localhost:5000/api/training/rollback \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "LocalLLM",
    "targetVersion": "lotto-predictor-v2.0",
    "reason": "Performance degradation detected",
    "immediate": true
  }'
```

## Confidence Scoring and Reasoning

### Confidence Score Calibration

#### Configuration
```env
# Confidence Scoring Settings
CONFIDENCE_SCORING_ENABLED=true
CONFIDENCE_MIN_THRESHOLD=0.0
CONFIDENCE_MAX_THRESHOLD=1.0
CONFIDENCE_CALIBRATION_SAMPLES=1000
CONFIDENCE_CALIBRATION_BINS=10

# Reasoning Configuration
REASONING_ENABLED=true
REASONING_MAX_LENGTH=500
REASONING_INCLUDE_FACTORS=true
REASONING_INCLUDE_STATISTICS=true
```

#### Calibration Process
```bash
# Automatic calibration triggers
1. Weekly calibration analysis
2. After model retraining
3. When calibration drift detected
4. Manual trigger via API

# Manual calibration
curl -X POST http://localhost:5000/api/confidence/calibrate \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "BedrockAgentCore",
    "samples": 1000,
    "recalibrate": true
  }'
```

### Reasoning Chain Implementation

#### Reasoning Components
```bash
# Frequency Analysis
- Historical number occurrence patterns
- Recent frequency trends
- Statistical significance of patterns

# Temporal Patterns
- Seasonal variations
- Day-of-week effects
- Holiday impacts
- Long-term cycles

# Statistical Correlations
- Number pair relationships
- Range distribution analysis
- Sum total patterns
- Even/odd ratios

# Model-Specific Insights
- AI pattern recognition
- Confidence factors
- Uncertainty quantification
```

#### API Response Format
```json
{
  "numbers": [5, 12, 18, 25, 33, 40],
  "confidenceScore": 0.92,
  "reasoningExplanation": "Based on frequency analysis of the last 100 draws, these numbers show strong temporal correlation with a 23% increase in occurrence over the past month. The combination exhibits optimal distribution across number ranges with historical precedent in similar seasonal patterns.",
  "keyFactors": [
    "frequency_analysis",
    "temporal_correlation",
    "seasonal_patterns",
    "range_distribution"
  ],
  "statisticalEvidence": {
    "frequencyScore": 0.87,
    "temporalScore": 0.94,
    "distributionScore": 0.89,
    "historicalPrecedent": 0.76
  },
  "uncertaintyFactors": [
    "Limited recent data for seasonal analysis",
    "High variance in recent draw patterns"
  ]
}
```

## Monitoring and Troubleshooting

### Health Monitoring

#### System Health Checks
```bash
# Overall system health
curl -X GET "http://localhost:5000/api/health" | jq

# Provider-specific health
curl -X GET "http://localhost:5000/api/predictions/providers/health" | jq

# Training pipeline health
curl -X GET "http://localhost:5000/api/training/health" | jq

# Local LLM service health
curl -X GET "http://localhost:8002/health" | jq
```

#### Performance Monitoring
```bash
# Provider performance metrics
curl -X GET "http://localhost:5000/api/predictions/providers/metrics" | jq

# Accuracy trends
curl -X GET "http://localhost:5000/api/accuracy/trends?timeframe=7d" | jq

# Training job status
curl -X GET "http://localhost:5000/api/training/status" | jq

# Resource utilization
curl -X GET "http://localhost:8002/metrics" | jq
```

### Troubleshooting Guide

#### Common Issues and Solutions

**1. Bedrock Authentication Errors**
```bash
# Check AWS credentials
aws sts get-caller-identity

# Verify Bedrock access
aws bedrock list-foundation-models --region us-west-2

# Test model access
aws bedrock get-foundation-model \
  --model-identifier anthropic.claude-3-sonnet-20240229-v1:0 \
  --region us-west-2
```

**2. Local LLM Memory Issues**
```bash
# Check memory usage
docker stats local-llm

# Reduce memory usage
LOCAL_LLM_QUANTIZATION=int8
LOCAL_LLM_MAX_MEMORY_GB=4
LOCAL_LLM_BATCH_SIZE=1

# Enable memory mapping
LOCAL_LLM_ENABLE_MEMORY_MAPPING=true
```

**3. Training Pipeline Failures**
```bash
# Check training logs
docker-compose logs training-pipeline

# Verify training data
curl -X GET "http://localhost:5000/api/training/data/summary" | jq

# Check GPU availability
nvidia-smi

# Restart training with reduced parameters
TRAINING_BATCH_SIZE=2
TRAINING_GRADIENT_ACCUMULATION_STEPS=8
```

**4. Provider Fallback Issues**
```bash
# Check provider chain status
curl -X GET "http://localhost:5000/api/predictions/providers/health" | jq

# Test individual providers
curl -X GET "http://localhost:5000/api/predictions/enhanced?provider=BedrockAgentCore&count=1"
curl -X GET "http://localhost:5000/api/predictions/enhanced?provider=LocalLLM&count=1"

# Reset circuit breakers
curl -X POST "http://localhost:5000/api/predictions/providers/reset-circuit-breakers"
```

### Log Analysis

#### Key Log Locations
```bash
# Backend logs
docker-compose logs backend | grep -i "llm\|bedrock\|training"

# Local LLM logs
docker-compose logs local-llm

# Training pipeline logs
docker-compose logs training-pipeline

# System logs
journalctl -u docker -f | grep predict-lotto
```

#### Log Patterns to Monitor
```bash
# Success patterns
"Bedrock prediction completed successfully"
"Local LLM model loaded successfully"
"Training job completed with accuracy"

# Warning patterns
"Provider fallback triggered"
"Confidence calibration drift detected"
"Training job taking longer than expected"

# Error patterns
"Bedrock authentication failed"
"Local LLM out of memory"
"Training job failed with error"
```

This completes the comprehensive Advanced LLM Setup and Configuration Guide. The system is now fully documented with detailed setup instructions, configuration options, and troubleshooting procedures for all advanced LLM features.