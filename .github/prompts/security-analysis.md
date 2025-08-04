# Security Analysis for PR #{{PR_NUMBER}}

**PR Context:**
- Pull Request: #{{PR_NUMBER}} by @{{PR_AUTHOR}}
- Issue: {{ISSUE_REF}}
- Branch: {{SOURCE_BRANCH}} → {{TARGET_BRANCH}}
- Target: {{TARGET_BRANCH}} (Production deployment)

**Your Mission:**
Assess security implications of THIS PR's changes.

**Analysis Focus:**
1. New code for vulnerabilities
2. Dependencies added/updated
3. Authentication/authorization changes
4. Data handling modifications

**Security Data Available:**
- CodeQL analysis results
- Dependency vulnerability scan results
- Secrets detection findings
- Security policy compliance status

**Security Checks:**
- Input validation in new endpoints
- SQL injection risks
- XSS vulnerabilities
- Secrets/credentials exposure
- CORS/security header changes
- Authentication bypasses

**Output Format:**
### 🔒 Security Assessment
- Risk Level: [Critical/High/Medium/Low/None]
- Key findings summary

### 🚨 Critical Issues (Block Deployment)
- Vulnerabilities requiring immediate fix
- Exposed secrets or credentials
- Authentication/authorization flaws

### ⚠️ Security Concerns
- Medium risk issues
- Best practice violations
- Dependency vulnerabilities

### ✅ Remediation Steps
1. Specific fixes needed
2. Code examples where helpful
3. Commands to run

### 🛡️ Security Improvements
- Positive security changes
- Vulnerabilities fixed

**Deployment Decision: [DEPLOY/BLOCK/CONDITIONAL]**

Focus on security impact of THIS PR's changes only. Be specific about what was introduced or changed.