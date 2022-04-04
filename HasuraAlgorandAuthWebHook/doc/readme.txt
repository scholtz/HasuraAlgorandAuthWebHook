# Hasura algorand auth web hook

This api uses algorand ARC-0014 for authentication to hasura graphql server.

Webhook checks if user is authenticated using valid signed transaction within the range of validity of the min/max rounds.

