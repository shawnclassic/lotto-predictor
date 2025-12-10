# Implementation Plan

- [x] 1. Set up project structure and development environment





  - Create monorepo structure with backend/, frontend/, and predictor/ directories
  - Set up .gitignore with .NET, Node.js, Python, and Docker exclusions
  - Create docker-compose.yml with PostgreSQL, backend, frontend, and predictor services
  - Configure environment variables for database credentials and service URLs
  - _Requirements: 9.1, 9.2_

- [x] 2. Implement database foundation and core entities





  - [x] 2.1 Create EF Core DbContext and entity models


    - Implement LottoDraw entity with all required fields and attributes
    - Create NumberCombination entity for historical combinations
    - Implement Prediction entity for storing generated predictions
    - Configure LottoDbContext with PostgreSQL connection and indexes
    - _Requirements: 1.1, 4.3, 5.4_

  - [x] 2.2 Write property test for database initialization











    - **Property 16: Database initialization is complete**
    - **Validates: Requirements 9.3**

  - [x] 2.3 Set up EF Core migrations and database initialization


    - Create initial migration for all entities
    - Configure database seeding and migration application
    - Implement database connection retry logic with exponential backoff
    - _Requirements: 9.3_

- [x] 3. Implement CSV parsing and import services




  - [x] 3.1 Create CSV header mapping and parsing logic


    - Implement header transformation (space removal, PascalCase conversion)
    - Create interval mapping for statistical fields (1-Oct → OneToTen)
    - Build type conversion system for int, decimal, DateTime, string fields
    - _Requirements: 1.1, 1.2_

  - [x] 3.2 Write property test for CSV parsing















    - **Property 1: CSV parsing extracts valid data**
    - **Validates: Requirements 1.1, 1.2**

  - [x] 3.3 Implement LottoImportService with duplicate detection


    - Create ImportResult model for tracking added/skipped records
    - Implement duplicate detection using Draw number as primary key
    - Build batch processing with error handling and logging
    - Add validation for required fields and data types
    - _Requirements: 1.3, 1.4, 1.5_


  - [x] 3.4 Write property test for duplicate detection




    - **Property 2: Duplicate detection preserves data integrity**
    - **Validates: Requirements 1.3, 1.4, 2.2**

  - [x] 3.5 Write property test for error handling





    - **Property 3: Error handling maintains processing continuity**
    - **Validates: Requirements 1.5, 4.4**

- [-] 4. Create lottery data query services


  - [x] 4.1 Implement LottoQueryService for draw operations



    - Create DrawExistsAsync method for duplicate checking
    - Implement GetLatestDrawAsync with date-based ordering
    - Build LottoDrawDto for API responses
    - Add error handling for empty database scenarios
    - _Requirements: 2.1, 3.1, 3.2, 3.3_

  - [x] 4.2 Write property test for draw existence queries





    - **Property 4: Draw existence queries are accurate**
    - **Validates: Requirements 2.1**

  - [x] 4.3 Write property test for latest draw retrieval





    - **Property 5: Latest draw retrieval is consistent**
    - **Validates: Requirements 3.1, 3.2**

- [x] 5. Implement number combination processing





  - [x] 5.1 Create multi-format file parser (CSV, TXT, PDF)


    - Implement file type detection and routing
    - Build CSV parser for comma-separated combinations
    - Create TXT parser for line-based number sets
    - Add PDF text extraction for number combinations
    - _Requirements: 4.1_

  - [x] 5.2 Write property test for file format parsing



    - **Property 7: File format parsing is universal**
    - **Validates: Requirements 4.1**

  - [x] 5.3 Implement combination validation and storage


    - Create validation for exactly 6 unique integers in range [1, 40]
    - Implement NumberCombination entity storage with timestamps
    - Add batch processing for large combination uploads
    - Build error handling for invalid combinations
    - _Requirements: 4.2, 4.3, 4.4_

  - [x] 5.4 Write property test for combination validation






    - **Property 6: Number combination validation enforces constraints**
    - **Validates: Requirements 4.2**

  - [x] 5.5 Write property test for combination storage





    - **Property 18: Combination storage includes metadata**
    - **Validates: Requirements 4.3**

- [x] 6. Build frequency-based prediction engine





  - [x] 6.1 Implement frequency calculation service


    - Create number frequency analysis across historical combinations
    - Implement combination scoring by summing constituent number frequencies
    - Build ranking system for top N predictions
    - Add caching for frequently accessed frequency data
    - _Requirements: 5.1, 5.2, 5.3_

  - [x] 6.2 Write property test for frequency calculations



    - **Property 8: Frequency calculation is deterministic**
    - **Validates: Requirements 5.1, 5.2, 5.3**

  - [x] 6.3 Create prediction provider abstraction


    - Define IPredictionProvider interface with PredictAsync method
    - Implement FrequencyPredictionProvider as base implementation
    - Create PredictionResult model for standardized responses
    - Add provider identification and metadata tracking
    - _Requirements: 5.4, 6.1_

- [x] 7. Implement Python FastAPI prediction service





  - [x] 7.1 Create FastAPI service with prediction endpoint


    - Set up FastAPI application with CORS middleware
    - Implement POST /predict endpoint with request validation
    - Create Pydantic models for PredictRequest and PredictResponse
    - Add OpenAPI documentation with example schemas
    - _Requirements: 6.3_

  - [x] 7.2 Integrate ML and GPT prediction models


    - Implement scikit-learn regression model for ML predictions
    - Create OpenAI GPT integration with few-shot prompting
    - Build blended prediction combining ML and GPT results
    - Add error handling and timeout configuration
    - _Requirements: 6.3_

  - [x] 7.3 Write property test for FastAPI integration



    - **Property 10: FastAPI integration maintains data contract**
    - **Validates: Requirements 6.3**

- [-] 8. Create prediction provider chain and fallback system







  - [x] 8.1 Implement FastApiPredictionProvider


    - Create HTTP client for calling Python FastAPI service
    - Implement request/response serialization and validation
    - Add timeout and retry logic for network calls
    - Build error handling with meaningful error messages
    - _Requirements: 6.2, 6.3_

  - [x] 8.2 Create prediction service with provider fallback


    - Implement provider chain: AWS LLM → FastAPI → Frequency
    - Add circuit breaker pattern for external service failures
    - Create comprehensive prediction storage with source tracking
    - Build request/response payload logging for training data
    - _Requirements: 6.1, 6.2, 6.4_

  - [x] 8.3 Write property test for provider fallback chain






    - **Property 9: Prediction provider fallback chain works reliably**
    - **Validates: Requirements 6.1, 6.2**

  - [x] 8.4 Write property test for prediction persistence







    - **Property 11: Prediction persistence is comprehensive**
    - **Validates: Requirements 5.4, 6.4, 10.1, 10.4**

- [x] 9. Build .NET Core Web API controllers and endpoints





  - [x] 9.1 Create LottoController with upload and query endpoints


    - Implement POST /api/lotto/upload for CSV file uploads
    - Create GET /api/lotto/exists/{draw} for duplicate checking
    - Add GET /api/lotto/latest for latest draw retrieval
    - Configure file upload limits and validation
    - _Requirements: 1.1, 2.1, 3.1_

  - [x] 9.2 Implement CombinationsController for prediction operations


    - Create POST /api/combinations/upload for general file uploads
    - Add GET /api/combinations/predictions for prediction retrieval
    - Implement request validation and error handling
    - Configure async operations for all I/O
    - _Requirements: 4.1, 5.3_

  - [x] 9.3 Configure dependency injection and middleware


    - Register all services with appropriate lifetimes
    - Configure CORS for frontend integration
    - Add request logging and error handling middleware
    - Set up environment-based configuration
    - _Requirements: 9.2_

- [x] 9.4 Write property test for configuration management



  - **Property 15: Configuration uses environment variables**
  - **Validates: Requirements 9.2**

- [x] 10. Checkpoint - Ensure backend services are working





  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Create Vue.js frontend application structure




  - [x] 11.1 Set up Vue.js project with required dependencies


    - Initialize Vue 3 project with TypeScript support
    - Install Axios for HTTP client, Vuex/Pinia for state management
    - Configure build tools and development server
    - Set up CSS structure with global and component-specific styles
    - _Requirements: 8.1_

  - [x] 11.2 Create core Vue components


    - Implement FileUpload.vue with progress tracking and validation
    - Create PredictionsView.vue for displaying prediction results
    - Build SideMenu.vue for navigation and toast notifications
    - Add LatestDraw.vue for displaying recent lottery information
    - _Requirements: 7.1, 7.2, 8.2_

- [x] 12. Implement file upload functionality with progress tracking





  - [x] 12.1 Create upload service with progress monitoring


    - Implement XMLHttpRequest-based upload with progress events
    - Add file validation for type, size, and format requirements
    - Create duplicate detection using first CSV row analysis
    - Build upload cancellation and retry functionality
    - _Requirements: 7.1, 7.2, 2.2_

  - [x] 12.2 Write property test for upload progress tracking



    - **Property 12: Upload progress tracking is accurate**
    - **Validates: Requirements 7.1, 7.2, 7.3, 7.4**

  - [x] 12.3 Implement upload completion handling


    - Create success/error event emission from FileUpload component
    - Add toast notification system for upload feedback
    - Implement import statistics display (records added/skipped)
    - Build error message display with specific failure details
    - _Requirements: 7.3, 7.4, 8.4_

  - [x] 12.4 Write property test for toast notifications



    - **Property 14: Toast notifications respond to events**
    - **Validates: Requirements 8.4**

- [x] 13. Create prediction display and interaction features




  - [x] 13.1 Implement prediction request and display


    - Create dropdown for selecting prediction count (1-10)
    - Implement API calls to backend prediction endpoints
    - Build prediction table with combination index, numbers, and source
    - Add loading states and error handling for prediction requests
    - _Requirements: 8.2, 8.3, 5.3_

  - [x] 13.2 Write property test for prediction display




    - **Property 13: Prediction display includes required information**
    - **Validates: Requirements 8.2**

  - [x] 13.3 Create latest draw display component


    - Implement automatic latest draw retrieval after uploads
    - Display draw number, date, winning numbers, bonus, and powerball
    - Add formatting and styling for draw information
    - Handle empty database scenarios gracefully
    - _Requirements: 3.1, 3.2, 3.3_

- [-] 14. Implement comprehensive data preservation system


  - [x] 14.1 Add training data collection infrastructure


    - Enhance prediction storage to include raw request/response payloads
    - Implement comprehensive logging for all external service calls
    - Create data export functionality for future ML training
    - Add metadata tracking for prediction accuracy analysis
    - _Requirements: 10.1, 10.4_

  - [x] 14.2 Write property test for data preservation







    - **Property 17: Data preservation maintains historical integrity**
    - **Validates: Requirements 10.2, 10.3**

- [x] 15. Final integration and deployment preparation





  - [x] 15.1 Complete Docker containerization


    - Create Dockerfiles for backend, frontend, and predictor services
    - Configure docker-compose with proper networking and volumes
    - Add health checks and restart policies for all services
    - Test full stack deployment with Docker Compose
    - _Requirements: 9.1_

  - [x] 15.2 Create comprehensive README documentation


    - Document prerequisites and setup instructions
    - Add environment variable configuration guide
    - Include deployment instructions for local and cloud environments
    - Create API documentation and usage examples
    - _Requirements: 9.1, 9.4_

- [x] 16. Final Checkpoint - Complete system integration test





  - Ensure all tests pass, ask the user if questions arise.