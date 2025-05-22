const fs = require('fs');
const path = require('path');

// Path to the guidelines markdown file
const guidelinesPath = path.join(__dirname, '../.aiguidance/ai-guidelines.md');

// Read the current file content
let content = fs.readFileSync(guidelinesPath, 'utf8');

// Format the current date
const today = new Date();
const formattedDate = today.toISOString().split('T')[0];

// Replace the last updated line
const updatedContent = content.replace(
  /Last Updated: .*/g,
  `Last Updated: ${formattedDate}`
);

// Write the updated content back to the file
fs.writeFileSync(guidelinesPath, updatedContent);

console.log(`Updated timestamp to ${formattedDate}`);