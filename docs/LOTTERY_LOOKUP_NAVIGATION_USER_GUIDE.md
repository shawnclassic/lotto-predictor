# Lottery Lookup and Navigation - User Guide

## Overview

The Lottery Lookup and Navigation feature provides comprehensive search and analysis capabilities for historical New Zealand lottery data. This powerful tool allows you to explore patterns, analyze frequencies, and navigate through historical draws with ease.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Number Lookup](#number-lookup)
3. [Combination Search](#combination-search)
4. [Frequency Analysis](#frequency-analysis)
5. [Draw Navigation](#draw-navigation)
6. [Bookmarks and History](#bookmarks-and-history)
7. [Export Features](#export-features)
8. [Advanced Search](#advanced-search)
9. [Visualizations](#visualizations)
10. [Tips and Best Practices](#tips-and-best-practices)

## Getting Started

### Accessing the Features

1. **Navigate to the Application**: Open your web browser and go to the PredictLottoNZ application
2. **Main Navigation**: Use the navigation menu to access different lookup and analysis features:
   - **Lookup**: Search for specific numbers and combinations
   - **Frequency**: Analyze number frequency patterns
   - **Navigation**: Browse historical draws
   - **Advanced Search**: Complex search criteria

### System Requirements

- **Modern Web Browser**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Internet Connection**: Required for real-time data access
- **Screen Resolution**: Minimum 1024x768 (responsive design supports mobile devices)

## Number Lookup

### Single Number Search

Search for occurrences of a specific lottery number across all historical draws.

**How to Use:**
1. Navigate to the **Lookup** section
2. Enter a number between 1-40 in the search field
3. Click **Search** or press Enter
4. View results showing:
   - Draw number and date
   - Position in winning combination
   - Whether it was a bonus or powerball number
   - Full winning combination for context

**Example Results:**
```
Number 7 appeared in:
- Draw 1999 (Dec 7, 2024): Position 1, Numbers: [7, 14, 21, 28, 35, 42]
- Draw 1995 (Nov 30, 2024): Position 3, Numbers: [5, 12, 7, 25, 33, 40]
- Draw 1990 (Nov 23, 2024): Bonus Number, Numbers: [8, 15, 22, 29, 36, 39], Bonus: 7
```

### Multiple Number Search

Search for multiple numbers simultaneously to see their individual occurrence patterns.

**How to Use:**
1. Enter multiple numbers separated by commas (e.g., "7, 14, 21")
2. Select **Search All Numbers**
3. Results show combined occurrences for all searched numbers
4. Use filters to view results by specific numbers

**Features:**
- **Individual Results**: See each number's occurrence history
- **Combined View**: View all results in chronological order
- **Filtering**: Filter results by specific numbers from your search
- **Sorting**: Sort by date, draw number, or position

### Search Tips

- **Valid Range**: Only numbers 1-40 are valid for NZ Powerball
- **Auto-completion**: The system provides suggestions as you type
- **Recent Searches**: Access your recent searches from the history dropdown
- **Quick Numbers**: Use preset buttons for commonly searched numbers

## Combination Search

### Exact Combination Search

Find draws where your specific number combination appeared together.

**How to Use:**
1. Navigate to **Combination Search**
2. Enter 2-6 numbers (e.g., "7, 14, 21, 28, 35, 42")
3. Select **Find Exact Matches**
4. View results showing draws with your exact combination

**Result Information:**
- **Exact Matches**: Draws containing all your numbers
- **Draw Details**: Full winning combination, date, and prize information
- **Match Highlighting**: Your numbers are highlighted in the results

### Partial Match Search

Find draws with partial matches of your combination.

**How to Use:**
1. Enter your number combination
2. Select **Include Partial Matches**
3. Set minimum match count (default: 2 numbers)
4. View results ranked by match quality

**Match Types:**
- **6/6 Match**: Exact match (jackpot winner!)
- **5/6 Match**: Five numbers matched
- **4/6 Match**: Four numbers matched
- **3/6 Match**: Three numbers matched
- **2/6 Match**: Two numbers matched (minimum)

**Example Results:**
```
Searching: [7, 14, 21, 28, 35, 42]

Exact Matches (6/6):
- Draw 1999: [7, 14, 21, 28, 35, 42] - JACKPOT!

Partial Matches (5/6):
- Draw 1995: [7, 14, 21, 28, 35, 39] - Missing: 42, Extra: 39
- Draw 1990: [5, 14, 21, 28, 35, 42] - Missing: 7, Extra: 5

Partial Matches (4/6):
- Draw 1985: [7, 14, 21, 28, 33, 40] - Missing: 35,42, Extra: 33,40
```

### Combination Analysis

**Match Quality Scoring:**
- **Proximity Score**: How close non-matching numbers are to your numbers
- **Position Analysis**: Whether matches occurred in similar positions
- **Recency Weight**: More recent matches are highlighted

**Statistical Information:**
- **Probability**: Calculated odds of your combination
- **Historical Performance**: How often similar combinations have won
- **Pattern Analysis**: Identification of number patterns in your combination

## Frequency Analysis

### Number Range Analysis

Analyze how often numbers in specific ranges appear in lottery draws.

**How to Use:**
1. Navigate to **Frequency Analysis**
2. Define number ranges:
   - **Low Numbers**: 1-10
   - **Mid Numbers**: 11-30
   - **High Numbers**: 31-40
   - **Custom Range**: Define your own range
3. Select date range for analysis (optional)
4. View comprehensive frequency statistics

**Analysis Results:**
- **Total Occurrences**: How many times numbers in the range appeared
- **Percentage**: Percentage of total draws containing range numbers
- **Average per Draw**: Average number of range numbers per draw
- **Trend Analysis**: Whether the range is "hot" or "cold"

### Individual Number Frequency

Get detailed statistics for individual numbers.

**Statistics Provided:**
- **Total Occurrences**: Total times the number has appeared
- **Last Appearance**: Most recent draw containing the number
- **Longest Gap**: Longest period without appearing
- **Current Gap**: Draws since last appearance
- **Frequency Percentage**: Percentage of total draws
- **Hot/Cold Status**: Current trend classification

**Hot and Cold Numbers:**
- **Hot Numbers**: Appearing more frequently than average recently
- **Cold Numbers**: Appearing less frequently than average recently
- **Due Numbers**: Haven't appeared for longer than average gap

### Comparative Analysis

Compare frequency patterns across multiple ranges or time periods.

**Features:**
- **Side-by-Side Comparison**: Compare up to 4 different ranges
- **Time Period Analysis**: Compare different time periods
- **Seasonal Patterns**: Identify seasonal trends in number selection
- **Statistical Significance**: Determine if patterns are statistically meaningful

## Draw Navigation

### Sequential Navigation

Browse through historical lottery draws in chronological order.

**Navigation Controls:**
- **Previous Draw**: Go to the chronologically previous draw
- **Next Draw**: Go to the chronologically next draw
- **First Draw**: Jump to the oldest available draw
- **Latest Draw**: Jump to the most recent draw

**Navigation Information:**
- **Current Position**: "Draw 1999 of 2000" (example)
- **Date Range**: Span of available historical data
- **Progress Indicator**: Visual progress bar showing position in sequence

### Direct Navigation

Jump directly to specific draws by number or date.

**Jump by Draw Number:**
1. Click **Jump to Draw**
2. Enter the specific draw number
3. Click **Go** to navigate directly

**Jump by Date:**
1. Click **Jump to Date**
2. Select date using the date picker
3. System finds the closest available draw to that date

**Smart Navigation:**
- **Missing Draws**: System handles gaps in draw sequences
- **Invalid Dates**: Suggests closest available draw for invalid dates
- **Boundary Handling**: Clear indication when reaching first/last draws

### Navigation Context

Understanding your position in the historical sequence.

**Context Information:**
- **Current Draw**: Full details of the current draw
- **Position Indicator**: Your location in the total sequence
- **Boundary Status**: Whether you're at the beginning or end
- **Date Span**: Range of available historical data
- **Missing Draws**: Information about any gaps in the sequence

**Keyboard Shortcuts:**
- **Arrow Keys**: Navigate previous/next
- **Page Up/Down**: Jump by larger increments
- **Home/End**: Go to first/last draw
- **Ctrl+G**: Open "Jump to" dialog

## Bookmarks and History

### Creating Bookmarks

Save interesting draws for quick access later.

**How to Bookmark:**
1. Navigate to any draw you want to save
2. Click the **Bookmark** button (star icon)
3. Add a custom label (optional)
4. Add description notes (optional)
5. Click **Save Bookmark**

**Bookmark Information:**
- **Auto Labels**: System generates descriptive labels if none provided
- **Quick Access**: Bookmarks appear in sidebar for instant navigation
- **Organization**: Sort bookmarks by date, label, or creation time

### Managing Bookmarks

**Bookmark Management:**
- **Edit Labels**: Update bookmark names and descriptions
- **Delete Bookmarks**: Remove bookmarks with confirmation
- **Export Bookmarks**: Save bookmark list to file
- **Import Bookmarks**: Load previously saved bookmarks

**Bookmark Categories:**
- **Interesting Patterns**: Draws with unusual number patterns
- **High Matches**: Draws matching your favorite combinations
- **Research**: Draws saved for analysis purposes
- **Personal**: Draws with personal significance

### Search History

The system automatically tracks your search activities.

**History Features:**
- **Recent Searches**: Quick access to recent number and combination searches
- **Search Patterns**: Identify your most common search types
- **Rerun Searches**: Easily repeat previous searches
- **Clear History**: Remove search history for privacy

**History Information:**
- **Search Type**: Number lookup, combination search, frequency analysis
- **Search Criteria**: The numbers or ranges you searched for
- **Result Count**: How many results were found
- **Timestamp**: When the search was performed

## Export Features

### Export Formats

Export your search results and analysis data in multiple formats.

**Available Formats:**
- **CSV**: Spreadsheet-compatible format for data analysis
- **JSON**: Structured data format for developers
- **PDF**: Formatted report for printing and sharing
- **Excel**: Native Excel format with formatting

### Export Options

**What You Can Export:**
- **Search Results**: Number lookup and combination search results
- **Frequency Data**: Statistical analysis and frequency calculations
- **Navigation History**: Record of draws you've viewed
- **Bookmarks**: Your saved draws and annotations
- **Analysis Reports**: Comprehensive analysis summaries

**Export Customization:**
- **Date Range**: Limit exports to specific time periods
- **Data Fields**: Choose which information to include
- **Formatting**: Select formatting options for reports
- **File Naming**: Custom file names with timestamps

### Large Dataset Handling

**Progress Tracking:**
- **Progress Bar**: Visual indication of export progress
- **Estimated Time**: Time remaining for large exports
- **Background Processing**: Continue using the application while exporting
- **Email Notification**: Optional email when large exports complete

**File Management:**
- **Download Links**: Secure, time-limited download URLs
- **File Expiration**: Exported files are automatically cleaned up
- **Size Limits**: Guidance on maximum export sizes
- **Compression**: Large files are automatically compressed

## Advanced Search

### Complex Search Criteria

Build sophisticated searches using multiple criteria and logical operators.

**Search Criteria Builder:**
1. **Number Criteria**: Specific numbers or ranges
2. **Date Criteria**: Date ranges and time periods
3. **Frequency Criteria**: Minimum/maximum occurrence thresholds
4. **Pattern Criteria**: Even/odd ratios, consecutive numbers, sum ranges

**Logical Operators:**
- **AND**: All criteria must be met
- **OR**: Any criteria can be met
- **NOT**: Exclude specific criteria
- **Grouping**: Use parentheses for complex logic

**Example Advanced Searches:**
```
Find draws where:
- Numbers 7 AND 14 both appear
- Date is between Jan 1, 2024 AND Dec 31, 2024
- Sum of winning numbers is between 100 AND 200

Find draws where:
- (Numbers 1-10 appear at least 2 times) OR (Numbers 31-40 appear at least 3 times)
- NOT containing number 13
- Date is in the last 6 months
```

### Saved Search Configurations

Save complex search criteria for reuse.

**Saving Searches:**
1. Build your search criteria
2. Click **Save Search Configuration**
3. Provide a descriptive name
4. Add notes about the search purpose
5. Save for future use

**Managing Saved Searches:**
- **Quick Access**: Saved searches appear in dropdown menu
- **Edit Configurations**: Modify saved search criteria
- **Share Searches**: Export search configurations to share with others
- **Search Templates**: Use predefined search templates for common analyses

### Search Performance Optimization

**Tips for Faster Searches:**
- **Narrow Date Ranges**: Limit searches to specific time periods
- **Use Specific Criteria**: More specific searches run faster
- **Avoid Wildcards**: Exact criteria perform better than broad searches
- **Cache Utilization**: Repeated searches use cached results

## Visualizations

### Frequency Charts

Visual representations of number frequency data.

**Chart Types:**
- **Bar Charts**: Compare frequency across numbers or ranges
- **Line Charts**: Show frequency trends over time
- **Heat Maps**: Visual representation of hot and cold numbers
- **Pie Charts**: Proportion of occurrences by category

**Interactive Features:**
- **Zoom**: Zoom into specific time periods or number ranges
- **Filtering**: Filter data by various criteria
- **Tooltips**: Hover for detailed information
- **Export**: Save charts as images or include in reports

### Timeline Visualizations

Track patterns and trends over time.

**Timeline Features:**
- **Pattern Recognition**: Identify cyclical patterns
- **Trend Lines**: Statistical trend analysis
- **Seasonal Indicators**: Highlight seasonal variations
- **Event Markers**: Mark significant draws or events

**Customization Options:**
- **Time Granularity**: Daily, weekly, monthly, or yearly views
- **Data Overlay**: Combine multiple data series
- **Color Schemes**: Choose from various color palettes
- **Animation**: Animated progression through time periods

### Comparative Visualizations

Compare different aspects of lottery data side by side.

**Comparison Types:**
- **Number Range Comparisons**: Compare frequency across different ranges
- **Time Period Comparisons**: Compare different time periods
- **Pattern Comparisons**: Compare different number patterns
- **Provider Comparisons**: Compare prediction accuracy across providers

## Tips and Best Practices

### Effective Search Strategies

**Start Broad, Then Narrow:**
1. Begin with general searches to understand patterns
2. Use frequency analysis to identify interesting ranges
3. Focus on specific numbers or combinations based on findings
4. Use advanced search to refine and validate hypotheses

**Use Multiple Approaches:**
- **Frequency First**: Start with frequency analysis to identify trends
- **Pattern Recognition**: Look for patterns in winning combinations
- **Historical Context**: Consider time-based patterns and seasonality
- **Validation**: Use different search methods to validate findings

### Data Interpretation Guidelines

**Understanding Randomness:**
- **No Guarantees**: Past patterns don't guarantee future results
- **Statistical Significance**: Look for statistically meaningful patterns
- **Sample Size**: Ensure sufficient data for reliable analysis
- **Confirmation Bias**: Avoid seeing patterns where none exist

**Frequency Analysis Best Practices:**
- **Long-Term Trends**: Focus on long-term patterns rather than short-term fluctuations
- **Multiple Metrics**: Use multiple frequency metrics for comprehensive analysis
- **Context Matters**: Consider the total number of draws in your analysis
- **Regular Updates**: Frequency patterns change as new draws are added

### Performance Optimization

**Faster Searches:**
- **Use Bookmarks**: Save frequently accessed draws
- **Limit Date Ranges**: Narrow searches to relevant time periods
- **Cache Awareness**: Repeated searches use cached results
- **Batch Operations**: Group related searches together

**Efficient Workflow:**
- **Plan Your Analysis**: Define what you're looking for before starting
- **Use Saved Searches**: Create reusable search configurations
- **Export Strategically**: Export data for offline analysis when needed
- **Regular Cleanup**: Remove old bookmarks and clear search history periodically

### Troubleshooting Common Issues

**Search Not Returning Results:**
- **Check Number Range**: Ensure numbers are between 1-40
- **Verify Date Range**: Confirm dates fall within available data
- **Simplify Criteria**: Try broader search criteria
- **Clear Filters**: Remove any active filters that might be limiting results

**Slow Performance:**
- **Narrow Search Scope**: Reduce the range of data being searched
- **Check Internet Connection**: Ensure stable connection for real-time features
- **Clear Browser Cache**: Refresh cached data if results seem outdated
- **Use Smaller Date Ranges**: Limit searches to smaller time periods

**Export Issues:**
- **File Size Limits**: Large exports may take time or hit size limits
- **Format Compatibility**: Ensure your software can open the export format
- **Download Timeouts**: Large files may require multiple download attempts
- **Browser Settings**: Check browser download settings and permissions

### Getting Help

**Built-in Help:**
- **Tooltips**: Hover over interface elements for quick help
- **Help Icons**: Click help icons for detailed explanations
- **Guided Tours**: Use the guided tour for new users
- **Keyboard Shortcuts**: Press '?' to see available keyboard shortcuts

**Additional Resources:**
- **User Community**: Connect with other users for tips and strategies
- **Documentation**: Comprehensive technical documentation available
- **Support**: Contact support for technical issues
- **Updates**: Regular feature updates and improvements

---

*This user guide covers the core functionality of the Lottery Lookup and Navigation features. For technical documentation and API details, see the Developer Guide and API Documentation.*