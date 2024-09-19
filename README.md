# Scans Mover
## _A Simple Program To Manage Scanned Files_
Scans Mover makes it easy to handle scanned PDF documents.
The program assumes documents are managed based on document numbers and does not current handle non numeric identification.

### Features
- Automatically split documents.
- Automatically name the split documents.
- Automatically move the documents to the correct folders.

### Getting Started
1. Click on the Locations tab.
2. Set each location.
    - Scans is the base folder for all scanned documents.
    - All documents are expected to start here.
3. Click on the Details tab.
4. Set the prefix, pages per document, minimum and tolerance.
    - The prefix is used when naming the documents after splitting them.
    - Tolerance is allowance to be above or below the minimum when naming the documents.
5. Click Batch Split to split all PDF documents that start with the specified prefix.
6. Click Move Files to move the files to the appropriate folders.
    - Deliveries, Service Calls and POs are moved to [Specified Location] -> [YYYYMM] [Full Month Name] Folder -> [Full Month Name] [DD] Folder -> Delivery Number Folder.
    - RMAs are moved to Folder -> RR [Number]-[Number+99] Folder
