# ResReport (Public Demo)

This repository is a **stripped-down, portfolio version** of ResReport, an internal reporting automation tool I built for a hotel group (MainStay Suites/Choice Hotels).

## About This Repo

This is **not** the current, full-featured version of the project. It has been intentionally simplified and sanitized before being made public, for security and confidentiality reasons.

Specifically, this repo:
- **Does not reflect the current complexity or feature set** of the live tool
- Has had **authentication logic, API keys, internal endpoints, and business-specific data removed or replaced with placeholders**
- Is meant to **demonstrate the general architecture and approach**, not serve as a production-ready or fully functional copy

The live version integrates with real third-party hotel booking systems, internal authentication (Clerk), and generates live reports used daily by hotel staff. None of that sensitive integration is included here.

## What ResReport Does (In Production)

ResReport is a web-based internal tool that automates report generation for a multi-property hotel group:
- Pulls data from a third-party hotel booking system
- Auto-populates predefined report templates
- Used by 20+ internal staff across reception and sales
- Supports the corporate relations team in generating year-over-year revenue comparison reports for corporate clients

## Tech Stack

- **Frontend:** React (CDN deployment)
- **Backend:** .NET
- **Auth (production only, not included here):** Clerk

## Why a Separate Public Version?

The production repository contains real authentication configuration, internal API integrations, and data tied to an active business system. This public repo exists purely to showcase the project's structure and my development approach for portfolio purposes, without exposing anything that could compromise the security of the live system or the company's data.

If you have questions about the project or would like more detail on the full implementation, feel free to reach out.
