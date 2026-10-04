# Banking Management API Documentation 📖

Comprehensive reference for all REST API endpoints provided by the **BankingManagement** backend.

---

## 📌 Base URL & Headers

- **Base URL**: `https://localhost:7112`
- **Default Content-Type**: `application/json`

### Authentication Header
Endpoints marked with **🔒 Protected** require a JWT Bearer token in the `Authorization` header:
```http
Authorization: Bearer <your_access_token>
```
To obtain a token, call the [User Login](#2-user-login) endpoint.

---

## 📑 Summary of Endpoints

| Category | Method | Endpoint | Auth | Description |
|---|---|---|---|---|
| **Auth & Users** | `POST` | `/api/User/CreateUser` | Anonymous | Register a new user/admin |
| | `POST` | `/api/User/Login` | Anonymous | Authenticate & get JWT Bearer token |
| **Customers** | `POST` | `/api/Customer/add` | 🔒 Bearer | Create one or multiple customer records |
| | `GET` | `/api/Customer/GetAll` | 🔒 Bearer | Retrieve all customers |
| | `GET` | `/api/Customer/Get/{id}` | 🔒 Bearer | Get customer profile by ID |
| | `PUT` | `/api/Customer/Update/{id}` | 🔒 Bearer | Update customer profile |
| | `DELETE` | `/api/Customer/Delete/{id}` | 🔒 Bearer | Delete customer profile |
| **Accounts** | `POST` | `/api/Account/add` | 🔒 Bearer | Open a new bank account |
| | `GET` | `/api/Account/GetAll` | 🔒 Bearer | Retrieve all accounts |
| | `GET` | `/api/Account/GetById/{accountId}` | 🔒 Bearer | Get bank account details by ID |
| | `PUT` | `/api/Account/Update/{accountId}` | 🔒 Bearer | Update account status or balance |
| **Transactions** | `POST` | `/api/Transaction/Deposit` | None | Deposit funds into an account |
| | `POST` | `/api/Transaction/Withdraw` | None | Withdraw funds from an account |
| | `POST` | `/api/Transaction/Transfer` | None | Transfer funds between two accounts |
| | `GET` | `/api/Transaction/TransHistory/{accountId}` | None | Retrieve statement/history for an account |
| **Email Logs** | `GET` | `/api/EmailLog/GetAll` | None | Retrieve all transactional email logs |
| | `GET` | `/api/EmailLog/{id}` | None | Retrieve specific email log entry |
| **Dashboard** | `GET` | `/api/Dashboard/Stats` | 🔒 Bearer | Retrieve KPI metrics and recent transactions |
| **AI Assistant** | `POST` | `/api/Chat/Ask` | 🔒 Bearer | Send question to AI banking assistant |

---

## 1. Authentication & User Management

### 1.1 Create User / Register
Registers a new user or administrator into the system with hashed credentials.

- **Method**: `POST`
- **Endpoint**: `/api/User/CreateUser`
- **Authorization**: None (Public)
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`CreateUserDto`)
```json
{
  "userName": "johndoe",
  "email": "johndoe@example.com",
  "password": "StrongPassword@123",
  "role": "Admin"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userName` | `string` | Yes | Unique login username |
| `email` | `string` | Yes | Valid email address |
| `password` | `string` | Yes | Password to be hashed |
| `role` | `string` | Yes | User role (e.g. `Admin`, `Customer`) |

#### Responses
- **`200 OK`**:
  ```json
  {
    "userId": 1,
    "userName": "johndoe",
    "email": "johndoe@example.com",
    "role": "Admin",
    "lastLogin": null
  }
  ```
- **`400 Bad Request`**: `"Username already exists"` or `"Email already exists"`

---

### 1.2 User Login
Authenticates credentials and returns a signed JWT Access Token.

- **Method**: `POST`
- **Endpoint**: `/api/User/Login`
- **Authorization**: None (Public)
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`LoginDto`)
```json
{
  "userName": "johndoe",
  "password": "StrongPassword@123"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userName` | `string` | Yes | Account username |
| `password` | `string` | Yes | Account password |

#### Responses
- **`200 OK`**:
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwidW5pcXVlX25hbWUiOiJqb2huZG9lIiwiZW1haWwiOiJqb2huZG9lQGV4YW1wbGUuY29tIiwicm9sZSI6IkFkbWluIi..."
  }
  ```
- **`401 Unauthorized`**: `"Invalid Username or password"`
- **`400 Bad Request`**: Missing username or password

---

## 2. Customer Management

### 2.1 Add Customer(s)
Creates one or multiple customer records in bulk.

- **Method**: `POST`
- **Endpoint**: `/api/Customer/add`
- **Authorization**: 🔒 Bearer Token
- **Request Headers**:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

#### Request Body (`List<CreateCustomerDto>`)
> **Note**: Pass a JSON **Array** of customer objects.
```json
[
  {
    "name": "Jane Smith",
    "email": "jane.smith@example.com",
    "phone": "9876543210",
    "address": "456 Elm Street, Springfield"
  }
]
```

| Field | Type | Required | Constraints |
|---|---|---|---|
| `name` | `string` | Yes | Full customer name |
| `email` | `string` | Yes | Valid email address |
| `phone` | `string` | Yes | Exactly 10 digits (`^[0-9]{10}$`) |
| `address` | `string` | Yes | Physical residential address |

#### Responses
- **`200 OK`**:
  ```json
  [
    {
      "customerId": 10,
      "name": "Jane Smith",
      "email": "jane.smith@example.com",
      "phone": "9876543210",
      "address": "456 Elm Street, Springfield",
      "createdDate": "2026-10-04T12:00:00Z",
      "account": null
    }
  ]
  ```
- **`400 Bad Request`**: Validation error (e.g. invalid phone number format or empty list).

---

### 2.2 Get All Customers
- **Method**: `GET`
- **Endpoint**: `/api/Customer/GetAll`
- **Authorization**: 🔒 Bearer Token

#### Responses
- **`200 OK`**:
  ```json
  [
    {
      "customerId": 1,
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "9876543210",
      "address": "123 Main St",
      "createdDate": "2026-09-01T10:00:00Z",
      "account": {
        "accountId": 101,
        "accountNumber": 1000000001,
        "customerId": 1,
        "balance": 15000.00,
        "status": "Active",
        "customerName": "John Doe",
        "createdDate": "2026-09-01T10:05:00Z"
      }
    }
  ]
  ```

---

### 2.3 Get Customer By ID
- **Method**: `GET`
- **Endpoint**: `/api/Customer/Get/{id}`
- **Authorization**: 🔒 Bearer Token
- **Route Parameter**: `id` (`int`) - The unique ID of the customer (e.g. `/api/Customer/Get/1`)

#### Responses
- **`200 OK`**: Returns customer object matching `CustomerResponseDto`.
- **`404 Not Found`**: Customer ID does not exist.

---

### 2.4 Update Customer
- **Method**: `PUT`
- **Endpoint**: `/api/Customer/Update/{id}`
- **Authorization**: 🔒 Bearer Token
- **Route Parameter**: `id` (`int`) - Customer ID to update

#### Request Body (`UpdateCustomerDto`)
```json
{
  "name": "Jane Smith Updated",
  "email": "jane.updated@example.com",
  "phone": "9876543211",
  "address": "789 Pine Avenue, Springfield"
}
```

#### Responses
- **`200 OK`**: Returns updated `CustomerResponseDto`.
- **`404 Not Found`**: Customer not found.

---

### 2.5 Delete Customer
- **Method**: `DELETE`
- **Endpoint**: `/api/Customer/Delete/{id}`
- **Authorization**: 🔒 Bearer Token
- **Route Parameter**: `id` (`int`) - Customer ID to delete

#### Responses
- **`200 OK`**: Customer deleted.
- **`404 Not Found`**: Customer not found.

---

## 3. Account Management

### 3.1 Open / Add Bank Account
Creates a new bank account associated with an existing customer.

- **Method**: `POST`
- **Endpoint**: `/api/Account/add`
- **Authorization**: 🔒 Bearer Token
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`CreateAccountDto`)
```json
{
  "customerId": 1,
  "balance": 5000.00,
  "status": "Active"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `customerId` | `int` | Yes | Existing customer identifier |
| `balance` | `decimal` | Yes | Initial deposit balance |
| `status` | `string` | Yes | Status: `"Active"`, `"Suspended"`, or `"Closed"` |

#### Responses
- **`200 OK`**:
  ```json
  {
    "accountId": 102,
    "accountNumber": 1000000002,
    "customerId": 1,
    "balance": 5000.00,
    "status": "Active",
    "customerName": "Jane Smith",
    "createdDate": "2026-10-04T12:30:00Z"
  }
  ```
- **`400 Bad Request`**: Invalid input data.

---

### 3.2 Get All Accounts
- **Method**: `GET`
- **Endpoint**: `/api/Account/GetAll`
- **Authorization**: 🔒 Bearer Token

#### Responses
- **`200 OK`**: Array of `AccountResponseDto` objects.

---

### 3.3 Get Account By ID
- **Method**: `GET`
- **Endpoint**: `/api/Account/GetById/{accountId}`
- **Authorization**: 🔒 Bearer Token
- **Route Parameter**: `accountId` (`int`) - The ID of the bank account

#### Responses
- **`200 OK`**: `AccountResponseDto`
- **`404 Not Found`**: Account does not exist.

---

### 3.4 Update Account
- **Method**: `PUT`
- **Endpoint**: `/api/Account/Update/{accountId}`
- **Authorization**: 🔒 Bearer Token
- **Route Parameter**: `accountId` (`int`) - Account ID

#### Request Body (`UpdateAccountDto`)
```json
{
  "balance": 7500.00,
  "status": "Active"
}
```

#### Responses
- **`200 OK`**: Updated `AccountResponseDto`.
- **`400 Bad Request`**: Invalid payload.

---

## 4. Transaction Management

All transaction endpoints automatically trigger transactional **email delivery via Resend** and persist an entry in the **EmailLog** table.

### 4.1 Deposit Funds
Credits an account with the specified amount.

- **Method**: `POST`
- **Endpoint**: `/api/Transaction/Deposit`
- **Authorization**: None (or as configured)
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`DepositDto`)
```json
{
  "accountId": 101,
  "amount": 2500.00,
  "description": "Salary deposit"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `accountId` | `int` | Yes | Target account ID |
| `amount` | `decimal` | Yes | Amount to credit (> 0) |
| `description` | `string?` | No | Optional remarks/memo |

#### Responses
- **`200 OK`**: `"Amount deposited Successfully"`
- **`404 Not Found`**: `"Account not found"`
- **`400 Bad Request`**: Validation error

---

### 4.2 Withdraw Funds
Debits an account with the specified amount, verifying sufficient balance.

- **Method**: `POST`
- **Endpoint**: `/api/Transaction/Withdraw`
- **Authorization**: None
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`WithdrawalDto`)
```json
{
  "accountId": 101,
  "amount": 500.00,
  "description": "ATM withdrawal"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `accountId` | `int` | Yes | Target account ID |
| `amount` | `decimal` | Yes | Amount to debit (> 0) |
| `description` | `string?` | No | Optional remarks/memo |

#### Responses
- **`200 OK`**: `"Amount Withdrawn Successfully"`
- **`400 Bad Request`**: `"Insufficient balance."` or `"Withdrawal amount must be greater than zero"`
- **`404 Not Found`**: `"Account not found"`

---

### 4.3 Transfer Funds Between Accounts
Transfers funds atomically from one account to another.

- **Method**: `POST`
- **Endpoint**: `/api/Transaction/Transfer`
- **Authorization**: None
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`TransferDto`)
```json
{
  "fromAccountId": 101,
  "toAccountId": 102,
  "amount": 1000.00,
  "description": "Rent transfer"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `fromAccountId` | `int` | Yes | Source account ID |
| `toAccountId` | `int` | Yes | Destination account ID |
| `amount` | `decimal` | Yes | Amount to transfer (> 0) |
| `description` | `string?` | No | Optional remarks |

#### Responses
- **`200 OK`**: `"Transfer successful."`
- **`400 Bad Request`**: `"Insufficient balance in source account."` or `"Cannot transfer to the same account."`
- **`404 Not Found`**: `"From account or To account not found."`

---

### 4.4 Get Transaction History
Retrieves past transactions for a specific account.

- **Method**: `GET`
- **Endpoint**: `/api/Transaction/TransHistory/{accountId}`
- **Authorization**: None
- **Route Parameter**: `accountId` (`int`) - Target account ID

#### Responses
- **`200 OK`**:
  ```json
  [
    {
      "transactionId": 501,
      "transactionType": "Deposit",
      "amount": 2500.00,
      "balanceAfterTransaction": 17500.00,
      "transactionDate": "2026-10-04T11:45:00Z",
      "description": "Salary deposit"
    },
    {
      "transactionId": 502,
      "transactionType": "Withdraw",
      "amount": 500.00,
      "balanceAfterTransaction": 17000.00,
      "transactionDate": "2026-10-04T12:00:00Z",
      "description": "ATM withdrawal"
    }
  ]
  ```

---

## 5. Notification & Email Logs

### 5.1 Get All Email Logs
Retrieves all sent transaction notification email records with delivery statuses.

- **Method**: `GET`
- **Endpoint**: `/api/EmailLog/GetAll`
- **Authorization**: None

#### Responses
- **`200 OK`**:
  ```json
  [
    {
      "id": 1,
      "recipientEmail": "customer@example.com",
      "recipientName": "John Doe",
      "subject": "Deposit Alert: ₹2,500.00 Credited",
      "message": "...",
      "transactionType": "Deposit",
      "accountId": 101,
      "accountNumber": 1000000001,
      "amount": 2500.00,
      "status": "Sent",
      "errorMessage": null,
      "sentAt": "2026-10-04T11:45:02Z",
      "resendEmailId": "re_msg_123456789"
    }
  ]
  ```

---

### 5.2 Get Email Log By ID
- **Method**: `GET`
- **Endpoint**: `/api/EmailLog/{id}`
- **Route Parameter**: `id` (`int`) - Email log record ID

#### Responses
- **`200 OK`**: Single `EmailLog` object
- **`404 Not Found`**: `"Email log with ID {id} not found."`

---

## 6. Dashboard Analytics

### 6.1 Get Executive Dashboard Statistics
Aggregates key real-time banking metrics and the 5 most recent transactions across the bank.

- **Method**: `GET`
- **Endpoint**: `/api/Dashboard/Stats`
- **Authorization**: 🔒 Bearer Token

#### Responses
- **`200 OK`**:
  ```json
  {
    "totalCustomers": 12,
    "totalAccounts": 15,
    "totalActiveAccounts": 14,
    "totalAmount": 345000.50,
    "totalTransactions": 84,
    "recentTransactions": [
      {
        "transactionId": 84,
        "accountId": 101,
        "accountNumber": 1000000001,
        "transactionType": "Deposit",
        "amount": 2500.00,
        "balanceAfterTransaction": 17500.00,
        "transactionDate": "2026-10-04T14:30:00Z",
        "description": "Salary deposit"
      }
    ]
  }
  ```

---

## 7. AI Banking Assistant

### 7.1 Ask AI Assistant
Interacts with the banking intelligence assistant for automated customer queries, balance inquiries, and help.

- **Method**: `POST`
- **Endpoint**: `/api/Chat/Ask`
- **Authorization**: 🔒 Bearer Token
- **Request Headers**: `Content-Type: application/json`

#### Request Body (`ChatRequestDto`)
```json
{
  "message": "How do I transfer funds to another account?"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `message` | `string` | Yes | The natural language query from the user |

#### Responses
- **`200 OK`** (`ChatResponseDto`):
  ```json
  {
    "reply": "To transfer funds, navigate to Transactions -> Transfer, enter the sender account ID, destination account ID, and amount.",
    "category": "Transactions",
    "structuredData": null,
    "suggestions": [
      "Check Account Balance",
      "View Recent Transactions",
      "How to open a new account"
    ],
    "timestamp": "2026-10-04T15:25:00Z"
  }
  ```
- **`400 Bad Request`**: `"Query message is required."`
