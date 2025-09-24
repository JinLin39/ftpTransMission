/**
 * Demo script showing how to use the Taiwan Postal Code CSV Processor
 * 
 * This script demonstrates:
 * 1. Processing sample CSV data
 * 2. Viewing grouped results
 * 3. Exporting regional CSV files
 */

const TaiwanPostalProcessor = require('./csvProcessor');
const fs = require('fs');
const path = require('path');

// Create sample CSV data for demonstration
const sampleCSVData = `郵遞區號,數量
100,50
101,75
102,30
200,120
220,80
300,95
320,40
400,200
407,150
500,90
600,110
700,180
800,60
900,25
950,35`;

console.log('=== Taiwan Postal Code CSV Processor Demo ===\n');

// Initialize processor
const processor = new TaiwanPostalProcessor();

// Process the sample CSV data
console.log('1. Processing sample CSV data...');
const result = processor.processCSV(sampleCSVData);

if (!result.success) {
    console.error('Processing failed:', result.error);
    process.exit(1);
}

console.log('✓ Processing completed successfully!\n');

// Display summary
console.log('2. Processing Summary:');
console.log(`   Total Records: ${result.summary.totalRecords}`);
console.log(`   Total Quantity: ${result.summary.totalQuantity}`);
console.log(`   Active Regions: ${result.summary.activeRegions} out of 9\n`);

// Display region breakdown
console.log('3. Region Breakdown:');
for (let i = 1; i <= 9; i++) {
    const region = result.summary.regionBreakdown[i];
    if (region.records > 0) {
        console.log(`   Region ${i} (${region.name}):`);
        console.log(`     Records: ${region.records}`);
        console.log(`     Quantity: ${region.quantity} (${region.percentage}%)`);
    }
}
console.log();

// Show detailed data for regions with data
console.log('4. Detailed Regional Data:');
for (let i = 1; i <= 9; i++) {
    const regionData = result.groupedData[i];
    if (regionData.data.length > 0) {
        console.log(`\n   Region ${i} - ${regionData.regionName}:`);
        console.log(`     Total Quantity: ${regionData.totalQuantity}`);
        console.log(`     Unique Postal Codes: ${regionData.uniquePostalCodeCount}`);
        console.log(`     Average Quantity: ${regionData.statistics.averageQuantity}`);
        console.log(`     Min/Max Quantity: ${regionData.statistics.minQuantity}/${regionData.statistics.maxQuantity}`);
        console.log('     Data:');
        regionData.data.forEach(item => {
            console.log(`       ${item.postalCode}: ${item.quantity}`);
        });
    }
}

// Create output directory for CSV files
const outputDir = path.join(__dirname, 'output');
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir);
}

// Export regional CSV files
console.log('\n5. Exporting Regional CSV Files:');
for (let i = 1; i <= 9; i++) {
    const csvFile = result.csvExports[i];
    if (csvFile.recordCount > 0) {
        const filePath = path.join(outputDir, csvFile.filename);
        fs.writeFileSync(filePath, csvFile.content, 'utf8');
        console.log(`   ✓ ${csvFile.filename} - ${csvFile.recordCount} records, ${csvFile.totalQuantity} total quantity`);
    } else {
        console.log(`   - ${csvFile.filename} - No data (empty file created)`);
        const filePath = path.join(outputDir, csvFile.filename);
        fs.writeFileSync(filePath, csvFile.content, 'utf8');
    }
}

// Create a summary report
console.log('\n6. Creating Summary Report...');
const summaryReport = `Taiwan Postal Code Processing Summary
Generated at: ${result.processedAt}

Total Records Processed: ${result.summary.totalRecords}
Total Quantity: ${result.summary.totalQuantity}
Active Regions: ${result.summary.activeRegions}/9

Regional Distribution:
${Object.keys(result.summary.regionBreakdown).map(regionNum => {
    const region = result.summary.regionBreakdown[regionNum];
    return `Region ${regionNum} (${region.name}): ${region.records} records, ${region.quantity} quantity (${region.percentage}%)`;
}).join('\n')}

Files Generated:
${Object.keys(result.csvExports).map(regionNum => {
    const csvFile = result.csvExports[regionNum];
    return `- ${csvFile.filename}: ${csvFile.recordCount} records`;
}).join('\n')}
`;

fs.writeFileSync(path.join(outputDir, 'processing_summary.txt'), summaryReport, 'utf8');
console.log('   ✓ processing_summary.txt created');

console.log('\n=== Demo completed! ===');
console.log(`Check the 'output' directory for generated CSV files and summary.`);

// Demonstrate error handling
console.log('\n7. Error Handling Demo:');
console.log('   Testing invalid CSV data...');

const invalidCSV = `郵遞區號,數量
abc,50
999999,invalid
,100`;

const errorResult = processor.processCSV(invalidCSV);
if (!errorResult.success) {
    console.log(`   ✓ Error handling works: ${errorResult.error}`);
} else {
    console.log('   Warning: Expected error but processing succeeded');
    console.log(`   Processed ${errorResult.originalDataCount} valid records from invalid data`);
}