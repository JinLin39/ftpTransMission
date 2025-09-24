/**
 * Simple test suite for Taiwan Postal Code CSV Processor
 * 
 * This provides basic validation of core functionality.
 * Run with: node test.js
 */

const TaiwanPostalProcessor = require('./csvProcessor');

class SimpleTest {
    constructor() {
        this.passed = 0;
        this.failed = 0;
        this.processor = new TaiwanPostalProcessor();
    }

    test(description, testFunction) {
        try {
            console.log(`Testing: ${description}`);
            testFunction();
            console.log('  ✓ PASSED\n');
            this.passed++;
        } catch (error) {
            console.log(`  ✗ FAILED: ${error.message}\n`);
            this.failed++;
        }
    }

    assertEquals(actual, expected, message = '') {
        if (actual !== expected) {
            throw new Error(`${message} Expected: ${expected}, Got: ${actual}`);
        }
    }

    assertTrue(condition, message = '') {
        if (!condition) {
            throw new Error(message || 'Condition is false');
        }
    }

    summary() {
        console.log('=== Test Summary ===');
        console.log(`Passed: ${this.passed}`);
        console.log(`Failed: ${this.failed}`);
        console.log(`Total: ${this.passed + this.failed}`);
        
        if (this.failed === 0) {
            console.log('🎉 All tests passed!');
            return true;
        } else {
            console.log('❌ Some tests failed.');
            return false;
        }
    }
}

const test = new SimpleTest();

console.log('=== Taiwan Postal Code CSV Processor Tests ===\n');

// Test 1: Postal code validation
test.test('Postal code validation', () => {
    test.assertTrue(test.processor.isValidTaiwanPostalCode('100'), 'Valid 3-digit code');
    test.assertTrue(test.processor.isValidTaiwanPostalCode('10001'), 'Valid 5-digit code');
    test.assertTrue(test.processor.isValidTaiwanPostalCode('123456'), 'Valid 6-digit code');
    test.assertTrue(!test.processor.isValidTaiwanPostalCode('0123'), 'Invalid: starts with 0');
    test.assertTrue(!test.processor.isValidTaiwanPostalCode('12'), 'Invalid: too short');
    test.assertTrue(!test.processor.isValidTaiwanPostalCode('1234567'), 'Invalid: too long');
    test.assertTrue(!test.processor.isValidTaiwanPostalCode('abc'), 'Invalid: non-numeric');
    test.assertTrue(!test.processor.isValidTaiwanPostalCode(''), 'Invalid: empty');
});

// Test 2: Region extraction
test.test('Region extraction from postal code', () => {
    test.assertEquals(test.processor.getRegionFromPostalCode('100'), 1);
    test.assertEquals(test.processor.getRegionFromPostalCode('200'), 2);
    test.assertEquals(test.processor.getRegionFromPostalCode('300'), 3);
    test.assertEquals(test.processor.getRegionFromPostalCode('950'), 9);
});

// Test 3: Basic CSV parsing
test.test('Basic CSV parsing', () => {
    const csvData = `郵遞區號,數量
100,50
200,75`;
    
    const result = test.processor.parseCSV(csvData);
    test.assertEquals(result.length, 2, 'Should parse 2 records');
    test.assertEquals(result[0].postalCode, '100', 'First postal code');
    test.assertEquals(result[0].quantity, 50, 'First quantity');
    test.assertEquals(result[0].region, 1, 'First region');
});

// Test 4: CSV parsing with English headers
test.test('CSV parsing with English headers', () => {
    const csvData = `postal_code,quantity
100,50
200,75`;
    
    const result = test.processor.parseCSV(csvData);
    test.assertEquals(result.length, 2, 'Should parse 2 records with English headers');
});

// Test 5: Error handling for invalid CSV
test.test('Error handling for invalid CSV', () => {
    let errorThrown = false;
    try {
        test.processor.parseCSV('');
    } catch (error) {
        errorThrown = true;
    }
    test.assertTrue(errorThrown, 'Should throw error for empty CSV');

    errorThrown = false;
    try {
        test.processor.parseCSV('invalid,headers\n100,50');
    } catch (error) {
        errorThrown = true;
    }
    test.assertTrue(errorThrown, 'Should throw error for missing required columns');
});

// Test 6: Data grouping by region
test.test('Data grouping by region', () => {
    const data = [
        { postalCode: '100', quantity: 50, region: 1 },
        { postalCode: '101', quantity: 30, region: 1 },
        { postalCode: '200', quantity: 75, region: 2 }
    ];
    
    const grouped = test.processor.groupByRegion(data);
    
    test.assertEquals(grouped[1].data.length, 2, 'Region 1 should have 2 records');
    test.assertEquals(grouped[1].totalQuantity, 80, 'Region 1 total quantity');
    test.assertEquals(grouped[2].data.length, 1, 'Region 2 should have 1 record');
    test.assertEquals(grouped[2].totalQuantity, 75, 'Region 2 total quantity');
    test.assertEquals(grouped[3].data.length, 0, 'Region 3 should be empty');
});

// Test 7: CSV export functionality
test.test('CSV export functionality', () => {
    const regionData = {
        data: [
            { postalCode: '100', quantity: 50 },
            { postalCode: '101', quantity: 30 }
        ]
    };
    
    const csv = test.processor.regionToCSV(regionData);
    const expectedLines = csv.trim().split('\n');
    
    test.assertEquals(expectedLines[0], '郵遞區號,數量', 'Header line');
    test.assertEquals(expectedLines[1], '100,50', 'First data line');
    test.assertEquals(expectedLines[2], '101,30', 'Second data line');
});

// Test 8: Complete processing workflow
test.test('Complete processing workflow', () => {
    const csvData = `郵遞區號,數量
100,50
200,75
300,25`;
    
    const result = test.processor.processCSV(csvData);
    
    test.assertTrue(result.success, 'Processing should succeed');
    test.assertEquals(result.originalDataCount, 3, 'Should process 3 records');
    test.assertTrue(result.summary.totalQuantity === 150, 'Total quantity should be 150');
    test.assertTrue(result.csvExports[1].recordCount === 1, 'Region 1 should have 1 record');
});

// Test 9: Statistics calculation
test.test('Statistics calculation', () => {
    const data = [
        { postalCode: '100', quantity: 10, region: 1 },
        { postalCode: '101', quantity: 20, region: 1 },
        { postalCode: '102', quantity: 30, region: 1 }
    ];
    
    const grouped = test.processor.groupByRegion(data);
    const stats = grouped[1].statistics;
    
    test.assertEquals(stats.count, 3, 'Count should be 3');
    test.assertEquals(stats.minQuantity, 10, 'Min quantity should be 10');
    test.assertEquals(stats.maxQuantity, 30, 'Max quantity should be 30');
    test.assertEquals(stats.averageQuantity, 20, 'Average quantity should be 20');
});

// Test 10: Data validation and warnings
test.test('Data validation handles invalid records gracefully', () => {
    const csvData = `郵遞區號,數量
100,50
abc,30
999999,invalid
200,75`;
    
    const result = test.processor.parseCSV(csvData);
    // Should only parse valid records (100,50 and 200,75)
    test.assertEquals(result.length, 2, 'Should parse only valid records');
});

// Run all tests
const success = test.summary();
process.exit(success ? 0 : 1);