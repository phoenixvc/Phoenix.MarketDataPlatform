const fs = require("fs");
const path = require("path");

try {
  // Path to the guidelines markdown file
  const guidelinesPath = path.join(
    __dirname,
    "../.aiguidance/ai-guidelines.md",
  );

  // Check if file exists before attempting to read
  if (!fs.existsSync(guidelinesPath)) {
    console.error(`File does not exist: ${guidelinesPath}`);
    process.exit(1);
  }

  // Read the current file content
  const content = fs.readFileSync(guidelinesPath, "utf8");

  // Format the current date
  const today = new Date();
  const formattedDate = today.toISOString().split("T")[0];

  // Replace the first occurrence of the last updated line (no global flag)
  const updatedContent = content.replace(
    /Last Updated: .*/, // no 'g' flag
    `Last Updated: ${formattedDate}`,
  );

  // Check if content was actually modified
  if (content === updatedContent) {
    console.warn('Warning: No "Last Updated" line found or no changes made');
  }

  // Write the updated content back to the file
  fs.writeFileSync(guidelinesPath, updatedContent);
  console.log(`Updated timestamp to ${formattedDate}`);
} catch (error) {
  console.error(`Error updating guidelines: ${error.message}`);
  process.exit(1);
}
