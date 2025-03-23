// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract Voting {
    struct Candidate {
        uint id;
        string name;
        uint voteCount;
    }

    address public admin;
    mapping(uint => Candidate) public candidates;
    mapping(address => bool) public hasVoted;
    uint public candidatesCount;

    uint public startTime;
    uint public endTime;
    bool public votingActive;

    event CandidateAdded(uint candidateID, string name);
    event VotingStarted(uint startTime, uint endTime);
    event VoteCast(address indexed voter, uint candidateID);
    event VotingEnded(uint endTime);

    modifier onlyAdmin() {
        require(msg.sender == admin, "Only admin can perform this action");
        _;
    }

    constructor() {
        admin = msg.sender;
    }

    // Добавление нового кандидата (все проверки можно проводить на сервере)
    function addCandidate(string memory name) external onlyAdmin {
        require(!votingActive, "Cannot add candidate during active voting session");
        candidatesCount++;
        candidates[candidatesCount] = Candidate(candidatesCount, name, 0);
        emit CandidateAdded(candidatesCount, name);
    }

    // Запуск голосования (минимум двух кандидатов)
    function startVoting(uint durationMinutes) external onlyAdmin {
        require(!votingActive, "Voting already active");
        require(candidatesCount >= 2, "At least two candidates required");
        startTime = block.timestamp;
        endTime = block.timestamp + (durationMinutes * 1 minutes);
        votingActive = true;
        emit VotingStarted(startTime, endTime);
    }

    // Функция голосования
    function vote(uint candidateID) external {
        require(votingActive, "Voting session is not active");
        require(block.timestamp >= startTime && block.timestamp <= endTime, "Voting period inactive");
        require(!hasVoted[msg.sender], "Already voted");
        require(candidateID > 0 && candidateID <= candidatesCount, "Invalid candidate ID");

        hasVoted[msg.sender] = true;
        candidates[candidateID].voteCount++;
        emit VoteCast(msg.sender, candidateID);
    }

    // Завершение голосования
    function endVoting() external onlyAdmin {
        require(votingActive, "Voting session not active");
        votingActive = false;
        emit VotingEnded(block.timestamp);
        // Вычисление победителя происходит на сервере путем анализа событий и состояния кандидатов.
    }
}
