/**
 * Taiwan Postal Code CSV Processor
 * 
 * This module provides functionality to:
 * 1. Read and parse CSV files containing Taiwan postal codes and quantities
 * 2. Group data by postal code regions (first digit 1-9)
 * 3. Export grouped data to separate CSV files
 * 
 * Taiwan Postal Code System:
 * - First digit represents major regions (1-9)
 * - Each region covers different cities/counties
 * 
 * Usage:
 * - Node.js: const processor = require('./csvProcessor');
 * - Browser: Include as script tag and use window.TaiwanPostalProcessor
 */

(function(global) {
    'use strict';

    /**
     * Taiwan Postal Code CSV Processor Class
     */
    class TaiwanPostalProcessor {
        constructor() {
            // Taiwan postal code regions mapping
            this.regionMap = {
                1: '台北市、新北市、基隆市、宜蘭縣',
                2: '台北市、新北市、桃園市、新竹縣市',
                3: '桃園市、新竹縣市、苗栗縣',
                4: '台中市、彰化縣、南投縣',
                5: '彰化縣、雲林縣、嘉義縣市',
                6: '嘉義縣市、台南市',
                7: '台南市、高雄市、屏東縣',
                8: '高雄市、屏東縣',
                9: '宜蘭縣、花蓮縣、台東縣、澎湖縣、金門縣、連江縣'
            };
        }

        /**
         * Parse CSV text into array of objects
         * @param {string} csvText - Raw CSV text content
         * @param {string} delimiter - CSV delimiter (default: ',')
         * @returns {Array} Array of parsed row objects
         */
        parseCSV(csvText, delimiter = ',') {
            try {
                if (!csvText || typeof csvText !== 'string') {
                    throw new Error('Invalid CSV input: must be a non-empty string');
                }

                const lines = csvText.trim().split('\n');
                if (lines.length < 2) {
                    throw new Error('CSV must contain at least a header row and one data row');
                }

                // Parse header row
                const headers = lines[0].split(delimiter).map(header => header.trim().replace(/"/g, ''));
                
                // Validate required columns
                const postalCodeColumn = headers.findIndex(h => 
                    h === '郵遞區號' || h === 'postal_code' || h === 'PostalCode'
                );
                const quantityColumn = headers.findIndex(h => 
                    h === '數量' || h === 'quantity' || h === 'Quantity'
                );

                if (postalCodeColumn === -1) {
                    throw new Error('Missing required column: 郵遞區號 (postal code)');
                }
                if (quantityColumn === -1) {
                    throw new Error('Missing required column: 數量 (quantity)');
                }

                // Parse data rows
                const data = [];
                for (let i = 1; i < lines.length; i++) {
                    const line = lines[i].trim();
                    if (!line) continue; // Skip empty lines

                    const values = line.split(delimiter).map(value => value.trim().replace(/"/g, ''));
                    if (values.length !== headers.length) {
                        console.warn(`Warning: Row ${i + 1} has ${values.length} columns, expected ${headers.length}`);
                        continue;
                    }

                    const postalCode = values[postalCodeColumn];
                    const quantity = values[quantityColumn];

                    // Validate postal code format (Taiwan postal codes are 3-6 digits)
                    if (!this.isValidTaiwanPostalCode(postalCode)) {
                        console.warn(`Warning: Invalid postal code "${postalCode}" at row ${i + 1}`);
                        continue;
                    }

                    // Validate quantity
                    const numQuantity = parseInt(quantity, 10);
                    if (isNaN(numQuantity) || numQuantity < 0) {
                        console.warn(`Warning: Invalid quantity "${quantity}" at row ${i + 1}`);
                        continue;
                    }

                    data.push({
                        postalCode: postalCode,
                        quantity: numQuantity,
                        region: this.getRegionFromPostalCode(postalCode),
                        originalRow: i + 1
                    });
                }

                return data;
            } catch (error) {
                throw new Error(`CSV parsing failed: ${error.message}`);
            }
        }

        /**
         * Validate Taiwan postal code format
         * @param {string} postalCode - Postal code to validate
         * @returns {boolean} True if valid Taiwan postal code
         */
        isValidTaiwanPostalCode(postalCode) {
            if (!postalCode || typeof postalCode !== 'string') return false;
            
            // Taiwan postal codes are 3-6 digits, starting with 1-9
            const cleanCode = postalCode.replace(/\D/g, ''); // Remove non-digits
            return /^[1-9]\d{2,5}$/.test(cleanCode);
        }

        /**
         * Extract region number from postal code (first digit)
         * @param {string} postalCode - Taiwan postal code
         * @returns {number} Region number (1-9)
         */
        getRegionFromPostalCode(postalCode) {
            const cleanCode = postalCode.replace(/\D/g, '');
            return parseInt(cleanCode.charAt(0), 10);
        }

        /**
         * Group parsed data by postal code regions
         * @param {Array} data - Array of parsed data objects
         * @returns {Object} Object with regions as keys, containing grouped data and statistics
         */
        groupByRegion(data) {
            try {
                if (!Array.isArray(data)) {
                    throw new Error('Data must be an array');
                }

                const grouped = {};
                
                // Initialize all regions (1-9)
                for (let i = 1; i <= 9; i++) {
                    grouped[i] = {
                        region: i,
                        regionName: this.regionMap[i],
                        data: [],
                        totalQuantity: 0,
                        uniquePostalCodes: new Set(),
                        statistics: {
                            count: 0,
                            averageQuantity: 0,
                            minQuantity: Infinity,
                            maxQuantity: -Infinity
                        }
                    };
                }

                // Group data by region
                data.forEach(item => {
                    const region = item.region;
                    if (region >= 1 && region <= 9) {
                        grouped[region].data.push(item);
                        grouped[region].totalQuantity += item.quantity;
                        grouped[region].uniquePostalCodes.add(item.postalCode);
                        
                        // Update statistics
                        const stats = grouped[region].statistics;
                        stats.count++;
                        stats.minQuantity = Math.min(stats.minQuantity, item.quantity);
                        stats.maxQuantity = Math.max(stats.maxQuantity, item.quantity);
                    }
                });

                // Calculate averages and finalize statistics
                for (let i = 1; i <= 9; i++) {
                    const regionData = grouped[i];
                    const stats = regionData.statistics;
                    
                    if (stats.count > 0) {
                        stats.averageQuantity = Math.round(regionData.totalQuantity / stats.count * 100) / 100;
                    } else {
                        stats.minQuantity = 0;
                        stats.maxQuantity = 0;
                    }
                    
                    // Convert Set to count for serialization
                    regionData.uniquePostalCodeCount = regionData.uniquePostalCodes.size;
                    delete regionData.uniquePostalCodes; // Remove Set for clean output
                }

                return grouped;
            } catch (error) {
                throw new Error(`Grouping failed: ${error.message}`);
            }
        }

        /**
         * Convert grouped data to CSV format for a specific region
         * @param {Object} regionData - Data for a specific region
         * @returns {string} CSV formatted string
         */
        regionToCSV(regionData) {
            if (!regionData || !regionData.data || regionData.data.length === 0) {
                return '郵遞區號,數量\n'; // Empty CSV with headers
            }

            let csv = '郵遞區號,數量\n';
            regionData.data.forEach(item => {
                csv += `${item.postalCode},${item.quantity}\n`;
            });

            return csv;
        }

        /**
         * Export all regions to separate CSV strings
         * @param {Object} groupedData - Data grouped by regions
         * @returns {Object} Object with region numbers as keys and CSV strings as values
         */
        exportRegionsToCSV(groupedData) {
            const csvFiles = {};
            
            for (let i = 1; i <= 9; i++) {
                const regionData = groupedData[i];
                csvFiles[i] = {
                    filename: `region_${i}_postal_data.csv`,
                    content: this.regionToCSV(regionData),
                    regionName: regionData.regionName,
                    totalQuantity: regionData.totalQuantity,
                    recordCount: regionData.data.length
                };
            }

            return csvFiles;
        }

        /**
         * Process CSV file completely: parse, group, and prepare exports
         * @param {string} csvText - Raw CSV content
         * @param {string} delimiter - CSV delimiter
         * @returns {Object} Complete processing results
         */
        processCSV(csvText, delimiter = ',') {
            try {
                console.log('Starting CSV processing...');
                
                // Step 1: Parse CSV
                const parsedData = this.parseCSV(csvText, delimiter);
                console.log(`Parsed ${parsedData.length} valid records`);

                // Step 2: Group by region
                const groupedData = this.groupByRegion(parsedData);
                console.log('Data grouped by regions');

                // Step 3: Prepare CSV exports
                const csvExports = this.exportRegionsToCSV(groupedData);
                console.log('CSV exports prepared');

                // Step 4: Generate summary
                const summary = this.generateSummary(groupedData);

                return {
                    success: true,
                    originalDataCount: parsedData.length,
                    groupedData: groupedData,
                    csvExports: csvExports,
                    summary: summary,
                    processedAt: new Date().toISOString()
                };

            } catch (error) {
                return {
                    success: false,
                    error: error.message,
                    processedAt: new Date().toISOString()
                };
            }
        }

        /**
         * Generate processing summary
         * @param {Object} groupedData - Data grouped by regions
         * @returns {Object} Summary statistics
         */
        generateSummary(groupedData) {
            const summary = {
                totalRecords: 0,
                totalQuantity: 0,
                activeRegions: 0,
                regionBreakdown: {}
            };

            for (let i = 1; i <= 9; i++) {
                const regionData = groupedData[i];
                summary.totalRecords += regionData.statistics.count;
                summary.totalQuantity += regionData.totalQuantity;
                
                if (regionData.statistics.count > 0) {
                    summary.activeRegions++;
                }

                summary.regionBreakdown[i] = {
                    name: regionData.regionName,
                    records: regionData.statistics.count,
                    quantity: regionData.totalQuantity,
                    percentage: 0 // Will calculate after total is known
                };
            }

            // Calculate percentages
            for (let i = 1; i <= 9; i++) {
                if (summary.totalQuantity > 0) {
                    summary.regionBreakdown[i].percentage = 
                        Math.round((summary.regionBreakdown[i].quantity / summary.totalQuantity) * 10000) / 100;
                }
            }

            return summary;
        }
    }

    // Export for different environments
    if (typeof module !== 'undefined' && module.exports) {
        // Node.js environment
        module.exports = TaiwanPostalProcessor;
    } else {
        // Browser environment
        global.TaiwanPostalProcessor = TaiwanPostalProcessor;
    }

})(typeof window !== 'undefined' ? window : global);