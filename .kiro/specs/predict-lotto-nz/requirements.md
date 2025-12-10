# Requirements Document

## Introduction

PredictLottoNZ is a full-stack web-based number prediction system that enables users to upload historical lottery data, generate frequency-based predictions, and integrate with machine learning services for enhanced prediction capabilities. The system consists of a Vue.js frontend, .NET Core Web API backend with PostgreSQL database, and a Python FastAPI prediction service.

## Glossary

- **System**: The complete PredictLottoNZ application including frontend, backend, and prediction services
- **LottoDraw**: A single lottery draw record containing winning numbers, date, and prize information
- **NumberCombination**: A set of 6 unique integers in the range [1, 40] representing a lottery selection
- **PredictionProvider**: A service component that generates number predictions using different algorithms
- **ImportResult**: A data structure containing counts of records added and skipped during CSV import
- **FastAPI_Service**: The Python-based prediction service that provides ML and GPT-based predictions

## Requirements

### Requirement 1

**User Story:** As a user, I want to upload historical lottery CSV files, so that I can build a database of past draws for prediction analysis.

#### Acceptance Criteria

1. WHEN a user uploads a Powerball NZ CSV file, THE System SHALL parse the file and extract lottery draw data
2. WHEN parsing CSV headers, THE System SHALL convert space-separated headers to PascalCase format
3. WHEN a CSV row contains a Draw number that already exists, THE System SHALL skip the duplicate record and continue processing
4. WHEN the import completes, THE System SHALL return an ImportResult showing records added and records skipped
5. WHERE the CSV file contains invalid data formats, THE System SHALL log parsing errors and continue with valid records

### Requirement 2

**User Story:** As a user, I want to check if a specific lottery draw already exists in the database, so that I can avoid uploading duplicate data.

#### Acceptance Criteria

1. WHEN a user queries for a specific draw number, THE System SHALL return whether that draw exists in the database
2. WHEN the frontend uploads a CSV file, THE System SHALL check the first draw number for duplicates before processing
3. IF a duplicate draw is detected during upload, THEN THE System SHALL prevent the upload and notify the user

### Requirement 3

**User Story:** As a user, I want to view the latest lottery draw information, so that I can see the most recent winning numbers and details.

#### Acceptance Criteria

1. WHEN a user requests the latest draw, THE System SHALL return the most recent LottoDraw by date
2. WHEN displaying draw information, THE System SHALL show draw number, date, six winning numbers, bonus number, and powerball
3. IF no draws exist in the database, THEN THE System SHALL return a not found response

### Requirement 4

**User Story:** As a user, I want to upload general number combination files in various formats, so that I can analyze historical patterns beyond official lottery data.

#### Acceptance Criteria

1. WHEN a user uploads a CSV, TXT, or PDF file, THE System SHALL parse the content for 6-number combinations
2. WHEN parsing number combinations, THE System SHALL validate that each combination contains exactly 6 unique integers between 1 and 40
3. WHEN storing combinations, THE System SHALL save them to a NumberCombination table with creation timestamp
4. WHERE invalid combinations are found, THE System SHALL skip them and continue processing valid ones

### Requirement 5

**User Story:** As a user, I want to generate frequency-based number predictions, so that I can see which combinations are most likely based on historical data.

#### Acceptance Criteria

1. WHEN generating predictions, THE System SHALL calculate frequency scores for each number based on historical combinations
2. WHEN ranking combinations, THE System SHALL score each candidate by summing the frequencies of its constituent numbers
3. WHEN a user requests N predictions, THE System SHALL return the top N highest-scoring combinations
4. WHEN storing predictions, THE System SHALL save each prediction with source type, timestamp, and generated numbers

### Requirement 6

**User Story:** As a system administrator, I want pluggable prediction providers, so that I can integrate different prediction algorithms and external services.

#### Acceptance Criteria

1. WHEN the System generates predictions, THE System SHALL attempt providers in order: AWS LLM, FastAPI service, then frequency-based
2. WHEN an external prediction service is unavailable, THE System SHALL fallback to the next available provider
3. WHEN calling the FastAPI_Service, THE System SHALL send weekly number data and receive ML, GPT, and blended predictions
4. WHEN any prediction is generated, THE System SHALL store it in the Prediction table with source identification

### Requirement 7

**User Story:** As a user, I want real-time upload progress tracking, so that I can monitor the status of large file uploads.

#### Acceptance Criteria

1. WHEN uploading files, THE System SHALL display a progress bar showing upload percentage from 0 to 100
2. WHEN upload is in progress, THE System SHALL show loading state indicators
3. WHEN upload completes successfully, THE System SHALL display a success message with import statistics
4. WHEN upload fails, THE System SHALL display an error message with failure details

### Requirement 8

**User Story:** As a user, I want to interact with a responsive web interface, so that I can easily upload files and view predictions across different devices.

#### Acceptance Criteria

1. WHEN the application loads, THE System SHALL display a clean interface with file upload and prediction viewing capabilities
2. WHEN displaying predictions, THE System SHALL show combination index, six numbers, and prediction source
3. WHEN users select prediction count, THE System SHALL provide a dropdown allowing selection of 1 to 10 predictions
4. WHEN import events occur, THE System SHALL display toast notifications for success and error states

### Requirement 9

**User Story:** As a developer, I want containerized deployment, so that I can easily deploy and scale the application across different environments.

#### Acceptance Criteria

1. WHEN deploying the application, THE System SHALL run all services using Docker containers via docker-compose
2. WHEN configuring services, THE System SHALL use environment variables for database credentials and external service URLs
3. WHEN the PostgreSQL service starts, THE System SHALL create the required database with proper migrations applied
4. WHERE AWS deployment is needed, THE System SHALL support configuration through environment variables and parameter store

### Requirement 10

**User Story:** As a data scientist, I want all predictions and uploads stored as training data, so that I can use this information for future machine learning model development.

#### Acceptance Criteria

1. WHEN any prediction is generated, THE System SHALL store the prediction with raw request and response payloads
2. WHEN lottery data is imported, THE System SHALL preserve all original draw information including prize divisions
3. WHEN number combinations are uploaded, THE System SHALL maintain historical records with timestamps
4. WHEN external services provide predictions, THE System SHALL store both input parameters and prediction results