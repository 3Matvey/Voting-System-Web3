// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

/// @title VotingSystem
/// @notice Контракт для проведения нескольких сессий голосования, с управлением кандидатами и голосованием.
contract VotingSystem {
    struct Candidate {
        uint id;
        string name;
        uint voteCount;
    }

    struct VotingSession {
        address sessionAdmin;
        uint startTime;
        uint endTime;
        bool votingActive;
        uint candidatesCount;  // идентификатор кандидата
        uint activeCandidatesCount; // фактическое число активных кандидатов
        mapping(uint => Candidate) candidates;
        mapping(address => bool) hasVoted;
    }

    address public immutable host; 
    uint public sessionCount;

    mapping(uint => VotingSession) private sessions;

    event SessionCreated(uint sessionId, address sessionAdmin);
    event CandidateAdded(uint sessionId, uint candidateId, string name);
    event CandidateRemoved(uint sessionId, uint candidateId, string name);
    event VotingStarted(uint sessionId, uint startTime, uint endTime);
    event VotingEnded(uint sessionId, uint endTime);
    /// @notice Событие, генерируемое при отдании голоса
    /// @param voter Адрес голосующего.
    event VoteCast(uint sessionId, address indexed voter, uint candidateId);


    /// @notice Вызов выполняется только админом конкретной сессии
    modifier onlyAdmin(uint sessionId) {
        require(msg.sender == sessions[sessionId].sessionAdmin, "Only session admin can perform this action");
        _;
    }

    /// @notice Вызов выполняется только хостом (суперадмином)
    modifier onlyHost() {
        require(msg.sender == host, "Only host can perfom this action");
        _;
    }

    constructor() {
        host = msg.sender;
    }

    /// @notice Создает новую сессию голосования.
    /// @param sessionAdmin Адрес администратора сессии.
    /// @return sessionId Идентификатор созданной сессии.
    function createSession(address sessionAdmin) external onlyHost returns (uint) {
        sessionCount++;
        VotingSession storage newSession = sessions[sessionCount];
        newSession.sessionAdmin = sessionAdmin;

        emit SessionCreated(sessionCount, sessionAdmin);

        return sessionCount;
    }

    /// @notice Добавляет кандидата в сессию голосования.
    /// @param sessionId Идентификатор сессии.
    /// @param name Имя кандидата.
    function addCandidate(uint sessionId, string memory name) external onlyAdmin(sessionId) {
        VotingSession storage session = sessions[sessionId];
        require(!session.votingActive, "Cannot add candidate during active voting session");

        session.candidatesCount++;
        session.activeCandidatesCount++;
        session.candidates[session.candidatesCount] = Candidate(session.candidatesCount, name, 0);

        emit CandidateAdded(sessionId, session.candidatesCount, name);
    }

    /// @notice Удаляет кандидата из сессии голосования.
    /// @param sessionId Идентификатор сессии.
    /// @param candidateId Идентификатор кандидата.
    function removeCandidate(uint sessionId, uint candidateId) external onlyAdmin(sessionId) {
        VotingSession storage session = sessions[sessionId];
        require(!session.votingActive, "Cannot remove candidate during active voting session");
        require(candidateId > 0 && candidateId <= session.candidatesCount, "Ivalid candidate ID");

        Candidate storage candidate = session.candidates[candidateId];
        require(bytes(candidate.name).length != 0, "Candidate does not exist");

        //сохранить имя для эмита
        string memory candidateName = candidate.name;
        delete session.candidates[candidateId];
        session.activeCandidatesCount--;

        emit CandidateRemoved(sessionId, candidateId, candidateName);
    }
 
    /// @notice Запускает голосование в сессии.
    /// @param sessionId Идентификатор сессии.
    /// @param durationMinutes Длительность голосования в минутах.
    function startVoting(uint sessionId, uint durationMinutes) external onlyAdmin(sessionId) {
        VotingSession storage session = sessions[sessionId];
        require(!session.votingActive, "Voting already active");
        require(session.activeCandidatesCount >= 2, "At least two candidates required");

        session.startTime = block.timestamp;
        session.endTime = block.timestamp + (durationMinutes * 1 minutes);
        session.votingActive = true;

        emit VotingStarted(sessionId, session.startTime, session.endTime);
    }

    /// @notice Отдает голос за кандидата в сессии.
    /// @param sessionId Идентификатор сессии.
    /// @param candidateId Идентификатор кандидата.
    function vote(uint sessionId, uint candidateId) external {
        VotingSession storage session = sessions[sessionId];
        require(session.votingActive, "Voting session is not active");
        require(block.timestamp >= session.startTime && block.timestamp <= session.endTime, "Voting period inactive");
        require(!session.hasVoted[msg.sender], "Already voted");
        require(candidateId > 0 && candidateId <= session.candidatesCount, "Invalid candidate ID");
        require(bytes(session.candidates[candidateId].name).length != 0, "Candidate does not exist");

        session.hasVoted[msg.sender] = true;
        session.candidates[candidateId].voteCount++;

        emit VoteCast(sessionId, msg.sender, candidateId);
    }

    /// @notice Завершает голосование в сессии.
    /// @param sessionId Идентификатор сессии.
    function endVoting(uint sessionId) external onlyAdmin(sessionId) {
        VotingSession storage session = sessions[sessionId];
        require(session.votingActive, "Voting session not active");

        session.votingActive = false;
        session.endTime = block.timestamp;

        emit VotingEnded(sessionId, session.endTime);
    }

    /// @notice Возвращает статус голосования для сессии.
    /// @param sessionId Идентификатор сессии.
    /// @return isActive true если голосование активно, иначе false.
    /// @return timeLeft Оставшееся время голосования в секундах (если активно, иначе 0).
    /// @return totalVotesCount Общее количество отданных голосов.
    function getVotingStatus(uint sessionId) external view returns (bool isActive, uint timeLeft, uint totalVotesCount) {
        VotingSession storage session = sessions[sessionId];
        isActive = session.votingActive && block.timestamp < session.endTime;
        timeLeft = isActive ? session.endTime - block.timestamp : 0;
        for (uint i = 1; i <= session.candidatesCount; i++) { 
            if (bytes(session.candidates[i].name).length > 0)
                totalVotesCount += session.candidates[i].voteCount;
        }
    }

    /// @notice Возвращает информацию о кандидате в сессии.
    /// @param sessionId Идентификатор сессии.
    /// @param candidateId Идентификатор кандидата.
    /// @return name Имя кандидата.
    /// @return voteCount Количество голосов за кандидата.
    function getCandidate(uint sessionId, uint candidateId) external view returns (string memory name, uint voteCount) {
        VotingSession storage session = sessions[sessionId];
        require(candidateId > 0 && candidateId <= session.candidatesCount, "Invalid candidate ID");
        Candidate storage candidate = session.candidates[candidateId];
        require(bytes(candidate.name).length != 0, "Candidate does not exist");
        return (candidate.name, candidate.voteCount);
    }
    
    /// @notice Возвращает сразу списком все активные кандидаты
    /// @param sessionId Идентификатор сессии.
    /// @return ids Идентификаторы кандидатов.
    /// @return names Имена кандидатов.
    /// @return voteCounts Количество голосов за каждого кандидата.
    function getActiveCandidates(uint sessionId) external view returns (uint[] memory ids, string[] memory names, uint[] memory voteCounts)
    {
        uint count = sessions[sessionId].activeCandidatesCount;
        ids         = new uint[](count);
        names       = new string[](count);
        voteCounts  = new uint[](count);

        uint ptr;
        for (uint i = 1; i <= sessions[sessionId].candidatesCount; i++) {
            Candidate storage c = sessions[sessionId].candidates[i];
            if (bytes(c.name).length == 0) 
                continue;
            ids[ptr]        = c.id;
            names[ptr]      = c.name;
            voteCounts[ptr] = c.voteCount;
            ptr++;  
        }
    }  
}