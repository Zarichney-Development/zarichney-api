# Security Policy

## Supported Versions

We actively support and provide security updates for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| main    | :white_check_mark: |
| develop | :white_check_mark: |

## Reporting a Vulnerability

We take security vulnerabilities seriously and appreciate your efforts to responsibly disclose any security issues you may find.

### How to Report

Please report security vulnerabilities by emailing **security@zarichney.com** with the following information:

1. **Description**: A clear description of the vulnerability
2. **Impact**: Potential impact and severity
3. **Reproduction**: Step-by-step instructions to reproduce the issue
4. **Environment**: System information where the vulnerability was discovered
5. **Proof of Concept**: Any proof-of-concept code (if applicable)

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your report within 48 hours
- **Initial Assessment**: We will provide an initial assessment within 5 business days
- **Updates**: We will keep you informed of our progress throughout the investigation
- **Resolution**: We aim to resolve critical vulnerabilities within 30 days
- **Credit**: We will acknowledge your contribution (unless you prefer to remain anonymous)

### Security Best Practices

When reporting vulnerabilities:

- **Do not** publicly disclose the vulnerability until we have had a chance to address it
- **Do not** access or modify user data during your security research
- **Do** provide sufficient detail to help us reproduce and understand the issue
- **Do** allow reasonable time for us to investigate and address the vulnerability

### Scope

This security policy applies to:

- The main Zarichney API application (`Code/Zarichney.Server/`)
- The Zarichney Website application (`Code/Zarichney.Website/`)
- Configuration files and deployment scripts
- Dependencies and third-party integrations

### Out of Scope

The following are generally out of scope:

- Social engineering attacks
- Physical security issues
- Denial of Service (DoS) attacks
- Issues in third-party applications or services not directly controlled by us

## Security Measures

We implement several security measures:

- **Dependency Scanning**: Automated scanning for vulnerable dependencies via Dependabot
- **Code Analysis**: Static code analysis with CodeQL
- **Secret Detection**: Automated detection of hardcoded secrets
- **Security Headers**: Implementation of security headers and best practices
- **Access Controls**: Role-based access control and authentication
- **Encryption**: Data encryption in transit and at rest

## Contact

For security-related questions or concerns, please contact:

**Email**: security@zarichney.com

Thank you for helping keep Zarichney secure!