# How to Convert to DOCX

The functional test cases have been created in the file:
**FUNCTIONAL_TEST_CASES.md**

## Option 1: Open in Microsoft Word (Recommended)
1. Open Microsoft Word
2. Click File > Open
3. Browse to: `c:\Users\MECHREVO\source\repos\financebank\FUNCTIONAL_TEST_CASES.md`
4. Word will automatically convert the Markdown to formatted text
5. Click File > Save As
6. Choose format: "Word Document (*.docx)"
7. Save as: `FINEBANK_Functional_Test_Cases.docx`

## Option 2: Use Online Converter
1. Go to: https://www.markdowntopdf.com/ or https://dillinger.io/
2. Upload or paste the content from FUNCTIONAL_TEST_CASES.md
3. Export as DOCX or PDF

## Option 3: Use Pandoc (if installed)
```powershell
pandoc FUNCTIONAL_TEST_CASES.md -o FINEBANK_Functional_Test_Cases.docx
```

## Test Cases Summary
- **Total Test Cases:** 71 Functional Test Cases
- **Modules Covered:** 7 (Authentication, Admin, Teller, Accountant, Finance Manager, Registrar, Customer)
- **Format:** Professional test case format with objectives, prerequisites, steps, and expected results

## What's Included:
✅ Login & Authentication (4 test cases)
✅ Admin Module (13 test cases)
✅ Teller Module (12 test cases)  
✅ Accountant Module (11 test cases)
✅ Finance Manager Module (12 test cases)
✅ Registrar Module (3 test cases)
✅ Customer Portal (16 test cases)

Each test case includes:
- Test Case ID
- Objective
- Prerequisites
- Detailed test steps
- Expected results
- Test data (where applicable)
