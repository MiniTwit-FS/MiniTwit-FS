console.log("Inject config script is running...");

const fs = require('fs');
const path = require('path');
// Rest of your code...

// Get the environment (defaults to 'development' if not set)
const environment = process.env.NODE_ENV || 'dev';

// Determine the correct appsettings file based on the environment
const appSettingsFile = 'appsettings.' + environment + '.json';
const appSettingsPath = path.join(__dirname, appSettingsFile);

// Path to the index.html that will be modified
const indexPath = path.join(__dirname, 'wwwroot', 'index.html');

// Check if the correct appsettings file exists
if (!fs.existsSync(appSettingsPath)) {
    console.error(`Error: Could not find ${appSettingsPath}`);
    process.exit(1);
}

// Read and parse the appsettings file
const appSettings = JSON.parse(fs.readFileSync(appSettingsPath, 'utf8'));

// Extract the API endpoint from appsettings.json (adjust the key based on your actual structure)
const apiEndpoint = appSettings.API_ENDPOINT;

// Check if index.html exists
if (!fs.existsSync(indexPath)) {
    console.error(`Error: Could not find ${indexPath}`);
    process.exit(1);
}

// Read the contents of index.html
const indexFile = fs.readFileSync(indexPath, 'utf8');

// Replace the placeholder in index.html with the actual API endpoint
const updatedIndex = indexFile.replace('{{API_ENDPOINT}}', apiEndpoint);

// Write the updated content back to index.html
fs.writeFileSync(indexPath, updatedIndex);

console.log(`Successfully injected API endpoint: ${apiEndpoint} for ${environment} environment`);
