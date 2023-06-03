# Account creation

Passwords are encrypted with BCrypt. Use an online tool like [BCrypt Online](https://bcrypt.online/) to encrypt your passwords. Passwords must be encrypted or the server won't be able to verify it.

## Create a new account

* Replace `SOME_GUID` with any valid guid which is not already used
* Replace `ACCOUNT_NAME` with the login name for the account
* Replace `YOUR_ENCRYPTED_PASSWORD` with the bcrypt string of your password

```sql
INSERT INTO account.accounts (Id, Username, Password, Email, Status, LastLogin, CreatedAt, UpdatedAt, DeleteCode) VALUES ('SOME_GUID', 'ACCOUNT_NAME', 'YOUR_ENCRYPTED_PASSWORD', 'some@mail.com', DEFAULT, null, DEFAULT, DEFAULT, DEFAULT);
```
