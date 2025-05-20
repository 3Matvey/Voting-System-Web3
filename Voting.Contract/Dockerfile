# Stage: use official Node.js LTS base image
FROM node:24-alpine AS truffle

# Install global Truffle and Ganache CLI for a local testnet
RUN npm install -g truffle ganache-cli

# Create workdir inside container
WORKDIR /blockchain

# Copy only config and migrations/contracts if you want persistence
# (Optionally mount your local ./contracts and ./migrations as volumes at runtime)
COPY truffle-config.js ./
COPY migrations/ ./migrations/
COPY build/contracts/ ./build/contracts/
COPY contracts/ ./contracts/

# Expose Ganache listening port (default 8545)
EXPOSE 8545

# Run Ganache CLI in deterministic mode for reproducible addresses
CMD ["ganache-cli", "--host", "0.0.0.0", "--port", "8545", "--networkId", "5777", "--deterministic"]
