import os
import json
from web3 import Web3

def main():
    # Параметры подключения (передаются из docker-compose через env vars)
    rpc_url       = os.getenv('BLOCKCHAIN_RPC_URL', 'http://ganache:8545')
    artifact_path = os.getenv(
        'ARTIFACT_PATH',
        '/blockchain/build/contracts/VotingSystem.json'
    )

    ws_url = rpc_url.replace('http://', 'ws://')

    # Подключаемся к ноде
    web3 = Web3(Web3.HTTPProvider(rpc_url))
    if not web3.isConnected():
        print(f"❌ Cannot connect to RPC at {rpc_url}")
        exit(1)

    # Получаем список аккаунтов
    accounts = web3.eth.accounts
    if not accounts:
        print("❌ No accounts returned by Ganache")
        exit(1)

    # Загружаем артефакт контракта и берём адрес из networks.development.address
    try:
        with open(artifact_path, 'r') as f:
            artifact = json.load(f)
        contract_address = artifact['networks']['development']['address']
    except Exception as e:
        print(f"❌ Failed to read contract address from {artifact_path}: {e}")
        exit(1)

    # Формируем JSON для API
    blockchain_config = {
        "Blockchain": {
            "RpcUrl":               rpc_url,
            "WsUrl":                ws_url,
            "ContractAddress":      contract_address,
            "DefaultSenderAddress": accounts[0]
        }
    }

    # Собираем список тест-юзеров (до 99 следующих аккаунтов)
    test_users = accounts[1:100]
    test_users_config = {
        "TestUsers": test_users
    }

    # Папка для вывода монтируется в docker-compose как volume config_vol
    out_dir = '/config'
    os.makedirs(out_dir, exist_ok=True)

    # Сохраняем оба файла
    with open(os.path.join(out_dir, 'blockchain-config.json'), 'w') as f:
        json.dump(blockchain_config, f, indent=2)
    print("✅ /config/blockchain-config.json written")

    with open(os.path.join(out_dir, 'test-users.json'), 'w') as f:
        json.dump(test_users_config, f, indent=2)
    print("✅ /config/test-users.json written")

if __name__ == '__main__':
    main()
