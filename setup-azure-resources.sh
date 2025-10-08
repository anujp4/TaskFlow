#!/bin/bash

# Variables - UPDATE THESE
RESOURCE_GROUP="taskflow-rg"
LOCATION="eastus"
SQL_SERVER_NAME="taskflow-sql-server"  # Must be globally unique
SQL_DB_NAME="TaskFlowDb"
SQL_ADMIN_USER="taskflowadmin"
SQL_ADMIN_PASSWORD="Sensex@00"  # Change this!
APP_SERVICE_PLAN="taskflow-plan"
WEB_APP_NAME="taskflow-api"  # Must be globally unique
KEY_VAULT_NAME="taskflow-kv"  # Must be globally unique
APP_INSIGHTS_NAME="taskflow-insights"

# Set default subscription (if you have multiple)
# az account set --subscription "Your Subscription Name"

echo "Creating Resource Group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "Creating SQL Server..."
az sql server create --name $SQL_SERVER_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --admin-user $SQL_ADMIN_USER --admin-password $SQL_ADMIN_PASSWORD

echo "Configuring SQL Server Firewall (Allow Azure Services)..."
az sql server firewall-rule create --resource-group $RESOURCE_GROUP --server $SQL_SERVER_NAME --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

echo "Adding your IP to SQL Server Firewall..."
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create --resource-group $RESOURCE_GROUP --server $SQL_SERVER_NAME --name AllowMyIP --start-ip-address 192.168.1.17 --end-ip-address 192.168.1.17

echo "Creating SQL Database..."
az sql db create --resource-group $RESOURCE_GROUP --server $SQL_SERVER_NAME --name $SQL_DB_NAME --service-objective S0 --backup-storage-redundancy Local

echo "Creating Key Vault..."
az keyvault create --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --enable-soft-delete true --soft-delete-retention-days 90

echo "Creating Application Insights..."
az monitor app-insights component create --app $APP_INSIGHTS_NAME --location $LOCATION --resource-group $RESOURCE_GROUP --application-type web

echo "Creating App Service Plan..."
az appservice plan create --name taskflow-plan --resource-group taskflow-rg --location eastus --sku B1 --is-linux

echo "Creating Web App..."
az webapp create --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP --plan $APP_SERVICE_PLAN --runtime "DOTNET|9.0"

echo "Configuring Web App to use System-Assigned Managed Identity..."
az webapp identity assign --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP

echo "Getting Managed Identity Object ID..."
MANAGED_IDENTITY_ID=$(az webapp identity show --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP --query principalId --output tsv)

echo "Granting Web App access to Key Vault..."
az keyvault set-policy --name $KEY_VAULT_NAME  --object-id $MANAGED_IDENTITY_ID --secret-permissions get list

echo "Getting connection strings and keys..."
SQL_CONNECTION_STRING="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DB_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

APP_INSIGHTS_KEY=$(az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query instrumentationKey --output tsv)

APP_INSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query connectionString --output tsv)

echo "Storing secrets in Key Vault..."
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "ConnectionStrings--DefaultConnection" --value "$SQL_CONNECTION_STRING"

az keyvault secret set --vault-name $KEY_VAULT_NAME --name "JwtSettings--Secret" --value "MySuperSecretKeyThatIsAtLeast32CharactersLong!"

az keyvault secret set --vault-name $KEY_VAULT_NAME --name "ApplicationInsights--InstrumentationKey" --value "$APP_INSIGHTS_KEY"

echo ""
echo "=========================================="
echo "Azure Resources Created Successfully!"
echo "=========================================="
echo "Resource Group: $RESOURCE_GROUP"
echo "SQL Server: $SQL_SERVER_NAME.database.windows.net"
echo "SQL Database: $SQL_DB_NAME"
echo "Web App: https://$WEB_APP_NAME.azurewebsites.net"
echo "Key Vault: $KEY_VAULT_NAME"
echo "Application Insights: $APP_INSIGHTS_NAME"
echo ""
echo "Connection String (for local testing):"
echo "$SQL_CONNECTION_STRING"
echo ""
echo "Application Insights Connection String:"
echo "$APP_INSIGHTS_CONNECTION_STRING"
echo "=========================================="