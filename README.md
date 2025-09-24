# Taiwan Postal Code CSV Processor

A JavaScript library for processing CSV files containing Taiwan postal codes and quantities. This module provides functionality to parse CSV data, group it by Taiwan postal code regions (1-9), and export regional data to separate CSV files.

## Features

- ✅ Parse CSV files with Taiwan postal codes and quantities
- ✅ Support both Chinese (郵遞區號, 數量) and English (postal_code, quantity) column headers
- ✅ Validate Taiwan postal code format (3-6 digits, starting with 1-9)
- ✅ Group data by postal code regions (first digit represents major regions)
- ✅ Generate statistics for each region (count, total, average, min/max quantities)
- ✅ Export each region to separate CSV files
- ✅ Comprehensive error handling and data validation
- ✅ Works in both Node.js and browser environments
- ✅ Detailed processing summaries and reports

## Taiwan Postal Code System

Taiwan uses a postal code system where the first digit represents major geographical regions:

- **Region 1**: 台北市、新北市、基隆市、宜蘭縣
- **Region 2**: 台北市、新北市、桃園市、新竹縣市  
- **Region 3**: 桃園市、新竹縣市、苗栗縣
- **Region 4**: 台中市、彰化縣、南投縣
- **Region 5**: 彰化縣、雲林縣、嘉義縣市
- **Region 6**: 嘉義縣市、台南市
- **Region 7**: 台南市、高雄市、屏東縣
- **Region 8**: 高雄市、屏東縣
- **Region 9**: 宜蘭縣、花蓮縣、台東縣、澎湖縣、金門縣、連江縣

## Installation

For Node.js environments:
```bash
# No external dependencies required
# Just include the csvProcessor.js file in your project
```

## Usage

### Node.js Environment

```javascript
const TaiwanPostalProcessor = require('./csvProcessor');
const fs = require('fs');

// Initialize processor
const processor = new TaiwanPostalProcessor();

// Read CSV file
const csvData = fs.readFileSync('data.csv', 'utf8');

// Process the CSV data
const result = processor.processCSV(csvData);

if (result.success) {
    console.log(`Processed ${result.originalDataCount} records`);
    console.log(`Total quantity: ${result.summary.totalQuantity}`);
    
    // Export regional CSV files
    for (let region = 1; region <= 9; region++) {
        const csvFile = result.csvExports[region];
        if (csvFile.recordCount > 0) {
            fs.writeFileSync(csvFile.filename, csvFile.content);
            console.log(`Exported ${csvFile.filename}`);
        }
    }
} else {
    console.error('Processing failed:', result.error);
}
```

### Browser Environment

```html
<!DOCTYPE html>
<html>
<head>
    <script src="csvProcessor.js"></script>
</head>
<body>
    <input type="file" id="csvFile" accept=".csv">
    <div id="results"></div>

    <script>
        const processor = new TaiwanPostalProcessor();
        
        document.getElementById('csvFile').addEventListener('change', function(event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const csvData = e.target.result;
                    const result = processor.processCSV(csvData);
                    
                    if (result.success) {
                        displayResults(result);
                    } else {
                        console.error('Processing failed:', result.error);
                    }
                };
                reader.readAsText(file);
            }
        });
        
        function displayResults(result) {
            const resultsDiv = document.getElementById('results');
            resultsDiv.innerHTML = `
                <h3>Processing Results</h3>
                <p>Total Records: ${result.originalDataCount}</p>
                <p>Total Quantity: ${result.summary.totalQuantity}</p>
                <p>Active Regions: ${result.summary.activeRegions}/9</p>
            `;
        }
    </script>
</body>
</html>
```

## CSV Format

### Required Columns

Your CSV file must contain these columns (supports both Chinese and English headers):

- **郵遞區號** or **postal_code**: Taiwan postal code (3-6 digits, starting with 1-9)
- **數量** or **quantity**: Numeric quantity value (integer, >= 0)

### Example CSV Format

```csv
郵遞區號,數量
100,50
101,75
200,120
300,95
407,150
```

or with English headers:

```csv
postal_code,quantity
100,50
101,75
200,120
300,95
407,150
```

## API Reference

### Class: TaiwanPostalProcessor

#### Methods

##### `parseCSV(csvText, delimiter = ',')`
Parse CSV text into an array of validated data objects.

**Parameters:**
- `csvText` (string): Raw CSV content
- `delimiter` (string): CSV delimiter (default: ',')

**Returns:** Array of objects with `{postalCode, quantity, region, originalRow}`

##### `groupByRegion(data)`
Group parsed data by postal code regions (1-9).

**Parameters:**
- `data` (Array): Array of parsed data objects

**Returns:** Object with regions 1-9 as keys, containing grouped data and statistics

##### `processCSV(csvText, delimiter = ',')`
Complete processing workflow: parse, group, and prepare exports.

**Parameters:**
- `csvText` (string): Raw CSV content  
- `delimiter` (string): CSV delimiter (default: ',')

**Returns:** Object containing:
```javascript
{
    success: boolean,
    originalDataCount: number,
    groupedData: Object,
    csvExports: Object,
    summary: Object,
    processedAt: string
}
```

##### `exportRegionsToCSV(groupedData)`
Export all regions to separate CSV strings.

**Parameters:**
- `groupedData` (Object): Data grouped by regions

**Returns:** Object with region CSV exports

##### `isValidTaiwanPostalCode(postalCode)`
Validate Taiwan postal code format.

**Parameters:**
- `postalCode` (string): Postal code to validate

**Returns:** Boolean indicating validity

## Testing

Run the included test suite:

```bash
npm test
# or
node test.js
```

## Demo

See the processor in action:

```bash
npm run demo
# or  
node demo.js
```

This will:
1. Process sample data
2. Display processing results
3. Create regional CSV files in the `output/` directory
4. Generate a summary report

## Error Handling

The processor includes comprehensive error handling:

- **Invalid CSV format**: Missing headers, malformed data
- **Invalid postal codes**: Non-numeric, wrong length, invalid format
- **Invalid quantities**: Non-numeric, negative values
- **Empty or malformed input**: Graceful error messages

Warnings are logged for invalid records, but processing continues with valid data.

## Output Files

When processing is complete, you'll get:

1. **Regional CSV files**: `region_1_postal_data.csv` through `region_9_postal_data.csv`
2. **Processing summary**: Detailed statistics and breakdown
3. **Console output**: Real-time processing information

## Browser Compatibility

- Modern browsers supporting ES6+ features
- IE 11+ (may require polyfills for some ES6 features)

## Node.js Compatibility

- Node.js 14+ recommended
- No external dependencies required

## License

MIT License - see LICENSE file for details

## Contributing

1. Fork the repository
2. Create your feature branch
3. Add tests for new functionality  
4. Ensure all tests pass
5. Submit a pull request

## Support

For issues or questions:
1. Check the test.js file for usage examples
2. Run the demo.js file to see expected behavior
3. Review error messages for debugging guidance