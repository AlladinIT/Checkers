using Domain;

namespace GameBrain;

public class CheckersBrain
{

    private readonly CheckersBoard _board;

    private int _amountOfEnemyPiecesTaken;
    
    public CheckersBrain(CheckersOption option, CheckersGameState? state)
    {

        //means a New Game was created
        if (state == null)
        {
            _board =  new CheckersBoard();
            InitializeNewGame(option);
        }
        // Using a state from database
        else
        {
            _board = System.Text.Json.JsonSerializer.Deserialize<CheckersBoard>(state.SerializedGameState)!;
        }
        
    }

    public string GetSerializedGameState()
    {
        return System.Text.Json.JsonSerializer.Serialize(_board);
    }
    
    private void InitializeNewGame(CheckersOption option)
    {
        var rowsOfPieces = option.RowsOfPieces;
        var boardWidth = option.Width;
        var boardHeight = option.Height;

        
        _board.GameBoard = new EGameSquareState[boardHeight][];
        for (int i = 0; i < boardHeight; i++)
        {
            _board.GameBoard[i] = new EGameSquareState[boardWidth];
        }
        
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if (i%2 == 1 && j%2 ==0 || i%2 == 0 && j%2 ==1)
                {
                    if (i < rowsOfPieces)
                    {
                        _board.GameBoard[i][j] = EGameSquareState.WhitePiece;
                    }
                    else if (i < _board.GameBoard.GetLength(0) && i >= _board.GameBoard.GetLength(0) - rowsOfPieces)
                    {
                        _board.GameBoard[i][j] = EGameSquareState.BlackPiece;
                    }
                    else
                    {
                        _board.GameBoard[i][j] = EGameSquareState.Dark;
                    }
                }

            }
        }
    }
    
    public EGameSquareState[][] GetBoard()
    {
        var jsonStr = System.Text.Json.JsonSerializer.Serialize(_board.GameBoard);
        return System.Text.Json.JsonSerializer.Deserialize<EGameSquareState[][]>(jsonStr)!;
    }
    
    
    
    public void MakeMove(int x1, int y1, int x2, int y2, bool nextMoveByBlack)
    {
        
        var pieceToMove = _board.GameBoard[x1][y1];
        _board.GameBoard[x1][y1] = EGameSquareState.Dark;
        
        //black piece reaches the first row on opponent's side
        if (nextMoveByBlack && x2 == 0)
        {
            _board.GameBoard[x2][y2] = EGameSquareState.BlackKing;
        }
        // white piece reaches the first row on opponent's side
        else if(nextMoveByBlack == false && x2 == _board.GameBoard.GetLength(0) - 1)
        {
            _board.GameBoard[x2][y2] = EGameSquareState.WhiteKing;
        }
        else
        {
            //if a piece to move was simple piece
            if (pieceToMove == EGameSquareState.BlackPiece || pieceToMove == EGameSquareState.WhitePiece)
            {
                _board.GameBoard[x2][y2] = nextMoveByBlack ? EGameSquareState.BlackPiece : EGameSquareState.WhitePiece;
            }
            //if a piece to move was a king
            else
            {
                _board.GameBoard[x2][y2] = nextMoveByBlack ? EGameSquareState.BlackKing : EGameSquareState.WhiteKing;
            }
            
        }
    }

    public void MakeAiMove(bool nextMoveByBlack, CheckersOption checkersOption)
    {
        
        var rnd = new Random();
        var possibleFirstClicks = new List<(int x, int y)>();

        for (var x = 0; x < _board.GameBoard.GetLength(0); x++)
        {

            for (var y = 0; y < _board.GameBoard[0].GetLength(0); y++)
            {
                if (FirstClickIsValid(x,y,nextMoveByBlack))
                {
                    possibleFirstClicks.Add((x,y));
                }
            }
        }
        
        
        var possibleSecondClicks = new List<(int x, int y)>();
        bool moveIsNotPossible = true;
        int x1;
        int y1;
        do
        {
            (x1, y1) = possibleFirstClicks[rnd.Next(0, possibleFirstClicks.Count)];
            for (var x = 0; x < _board.GameBoard.GetLength(0); x++)
            {

                for (var y = 0; y < _board.GameBoard[0].GetLength(0); y++)
                {
                    if (_board.GameBoard[x][y] != EGameSquareState.Dark) continue; //can only move to black square

                    //For stack overflow check (otherwise piece starts circling around through same positions)
                    var alreadyBeenOnThisSquare = new List<(int x, int y)>();
                    
                    if (isMovePossibleWhenOnlyMandatoryMovesAllowed(x1,y1,x,y,nextMoveByBlack,checkersOption,
                            false,alreadyBeenOnThisSquare))
                    {
                        possibleSecondClicks.Add((x,y));
                        moveIsNotPossible = false;
                    }
                }
            }

            if (moveIsNotPossible)
            {
                possibleFirstClicks.Remove((x1, y1));
                possibleSecondClicks.Clear();
            }


        } while (moveIsNotPossible);

        var (x2, y2) = possibleSecondClicks[rnd.Next(0, possibleSecondClicks.Count)];
        
        //Console.WriteLine("x1: "+x1+", y1: "+y1+", x2: "+x2+", y2: "+y2);
        if (isMovePossibleWhenOnlyMandatoryMovesAllowed(x1,y1,x2,y2,nextMoveByBlack,checkersOption,true,new List<(int x, int y)>()))
        {
            MakeMove(x1,y1,x2,y2,nextMoveByBlack);
            
        }
        
        
    }
    
    
    public bool IsMovePossible(int x1, int y1, int x2, int y2,
        bool nextMoveByBlack,CheckersOption checkersOption,bool calledFromController)
    {
        var boardWidth = _board.GameBoard[0].GetLength(0);
        var boardHeight = _board.GameBoard.GetLength(0);
        var pieceToMove = _board.GameBoard[x1][y1];
        if (x1 < 0 || y1 < 0 || x1 >= boardHeight || y1 >= boardWidth || 
            x2 < 0 || y2 < 0 || x2 >= boardHeight || y2 >= boardWidth) return false; //incorrect input from user
        
        if (_board.GameBoard[x2][y2] != EGameSquareState.Dark) return false; //can only move to black square

        //For stack overflow check (otherwise piece starts circling around through same positions)
        var alreadyBeenOnThisSquare = new List<(int x, int y)>();

        //IF CAPTURING IS MANDATORY => WE NEED TO CALCULATE BEST MOVE (Biggest number of enemy pieces to take)
        if (checkersOption.CapturingIsMandatory)
        {
            if (isMovePossibleWhenOnlyMandatoryMovesAllowed(x1, y1, x2, y2, nextMoveByBlack,checkersOption,
                    calledFromController, alreadyBeenOnThisSquare))
            {
                return true;
            }
        }
        
        //CAPTURING IS NOT MANDATORY
        else
        {
            //MOVING ORDINARY PIECE
            if (pieceToMove == EGameSquareState.BlackPiece || pieceToMove == EGameSquareState.WhitePiece)
            {
                if (IsMovePossibleForPiece(x1, y1, x2, y2, nextMoveByBlack,checkersOption,calledFromController,
                        true,0,0, alreadyBeenOnThisSquare))
                {
                    return true;
                }
            }
            //MOVING KING
            else
            {
                if (IsMovePossibleForKing(x1, y1, x2, y2, nextMoveByBlack,checkersOption,calledFromController,
                        true,0,0, alreadyBeenOnThisSquare))
                {
                    return true;
                }
            }
        }


        return false;
    }
    
    
    
    private bool IsMovePossibleForPiece(int x1, int y1, int x2, int y2,  bool nextMoveByBlack,
        CheckersOption checkersOption, bool calledFromController,bool firstTime,
        int xStepFromPrevDirection, int yStepFromPrevDirection, List<(int x, int y)> alreadyBeenOnThisSquare)
    {

        for (int xStep = -1; xStep <= 1; xStep++)
        {
            for (int yStep = -1; yStep <= 1; yStep++)
            {

                //skip Zeros and skip Steps to white squares and SKIP GOING BACK WHERE YOU CAME FROM
                if (xStep == 0 && yStep == 0 || 
                    xStep == -1 && yStep == 0 || xStep == 0 && yStep == 1 ||
                    xStep == 1 && yStep == 0 || xStep == 0 && yStep == -1 ||
                    xStep == -xStepFromPrevDirection && yStep == -yStepFromPrevDirection) continue;


                //CHECK IF ONLY FORWARD CAPTURING IS ALLOWED FOR ORDINARY PIECE
                if (!checkersOption.PiecesCapturingForwardAndBackward)
                {
                    if (nextMoveByBlack)
                    {
                        if (xStep == 1 && yStep == -1 || xStep == 1 && yStep == 1)
                        {
                            continue;
                        }
                    }

                    if (nextMoveByBlack == false)
                    {
                        if (xStep == -1 && yStep == -1 || xStep == -1 && yStep == 1)
                        {
                            continue;
                        }
                    }
                }
                
                var boardWidth = _board.GameBoard[0].GetLength(0);
                var boardHeight = _board.GameBoard.GetLength(0);
                
                var currentX = x1 + xStep;
                var currentY = y1 + yStep;
                
                
                //check for borders
                if (currentX < 0 || currentY < 0 || currentX >= boardHeight || currentY >= boardWidth)
                {
                    continue;
                }
                
                //check for no enemy piece around selected piece
                var enemyPiece = EGameSquareState.BlackPiece;
                var enemyKing = EGameSquareState.BlackKing;
                if (nextMoveByBlack)
                {
                    enemyPiece = EGameSquareState.WhitePiece;
                    enemyKing = EGameSquareState.WhiteKing;
                }
                if (_board.GameBoard[currentX][currentY] == enemyPiece 
                    || _board.GameBoard[currentX][currentY] == enemyKing)
                {
                    
                    currentX = currentX + xStep;
                    currentY = currentY + yStep;

                    //Check for border errors
                    if (currentX < 0 || currentY < 0 || currentX >= boardHeight || currentY >= boardWidth)
                    {
                        continue;
                    }
                    
                    //Checking if we have already been on this square (stack overflow check)
                    bool stackoverflow = false;
                    foreach (var square in alreadyBeenOnThisSquare)
                    {
                        if (currentX == square.x && currentY == square.y)
                        {
                            stackoverflow = true;
                        }
                    }
                    if (stackoverflow)
                    {
                        continue;
                    }
                    
                    //check if square is black
                    if (_board.GameBoard[currentX][currentY] == EGameSquareState.Dark )
                    {

                        //Memorizing the dark square, where we are at right now
                        alreadyBeenOnThisSquare.Add((currentX,currentY));
                        
                        
                        // checking if we found our destination [x2][y2]
                        if (currentX == x2 && currentY == y2)
                        {
                            if (calledFromController)
                            {
                                ChangeSquareState(currentX-xStep,currentY-yStep, EGameSquareState.Dark);
                            }
                            
                            //Increasing this number, knowing that this is our destination and move is possible
                            _amountOfEnemyPiecesTaken++;
                            
                            return true;
                        }

                        //repeat looking for destination location
                        if (IsMovePossibleForPiece(currentX, currentY, x2, y2, nextMoveByBlack,checkersOption,
                                calledFromController, false,xStep,yStep, alreadyBeenOnThisSquare))
                        {
                            if (calledFromController)
                            {
                                ChangeSquareState(currentX-xStep,currentY-yStep, EGameSquareState.Dark);
                            }
                            
                            //Increasing this number, knowing that we found our destination and move is possible
                            _amountOfEnemyPiecesTaken++;
                            
                            return true;
                        }
                        

                    }
                }
                else
                {
                    //next move by black
                    if (nextMoveByBlack)
                    {
                        if (x2 < x1 && x2 + 1 == x1 && (y1+1 == y2 || y1-1 == y2) && firstTime) //simple moving forward check
                        {
                            return true;
                        }
                    }
                    //next move by white
                    else
                    {
                        if (x2 > x1 && x1 + 1 == x2 && (y1+1 == y2 || y1-1 == y2) && firstTime)//simple moving forward check
                        {
                            return true;
                        }
                    }
                    
                }
                
            }
        }

        return false;
    }

    private bool IsMovePossibleForKing(int x1, int y1, int x2, int y2,  bool nextMoveByBlack,
        CheckersOption checkersOption, bool calledFromController,bool firstTime,
        int xStepFromPrevDirection, int yStepFromPrevDirection, List<(int x, int y)> alreadyBeenOnThisSquare)
    {
        for (int xStep = -1; xStep <= 1; xStep++)
        {
            for (int yStep = -1; yStep <= 1; yStep++)
            {
                //skip Zeros and skip Steps to white squares and SKIP GOING BACK WHERE YOU CAME FROM
                if (xStep == 0 && yStep == 0 || 
                    xStep == -1 && yStep == 0 || xStep == 0 && yStep == 1 ||
                    xStep == 1 && yStep == 0 || xStep == 0 && yStep == -1 ||
                    xStep == -xStepFromPrevDirection && yStep == -yStepFromPrevDirection) continue;

        
                var boardWidth = _board.GameBoard[0].GetLength(0);
                var boardHeight = _board.GameBoard.GetLength(0);
                
                var currentX = x1;
                var currentY = y1;
                var enemyPieceFound = false;
                int enemyPieceX = -1;
                int enemyPieceY = -1;
                
                //GOING THROUGH CHOSEN DIAGONAL
                do
                {

                    currentX = currentX + xStep;
                    currentY = currentY + yStep;
                    
                    
                    //check for borders
                    if (currentX < 0 || currentY < 0 || currentX >= boardHeight || currentY >= boardWidth)
                    {
                        break;
                    }

                    
                    //Checking if we have already been on this square (stack overflow check)
                    bool stackoverflow = false;
                    foreach (var square in alreadyBeenOnThisSquare)
                    {
                        if (currentX == square.x && currentY == square.y)
                        {
                            stackoverflow = true;
                        }
                    }
                    if (stackoverflow)
                    {
                        continue;
                    }
                    
                    
                    //if we met enemy piece in previous step
                    if (enemyPieceFound)
                    {
                        // if square after "enemyPiece" is occupied by PIECE OR KING   =>   can't take it 
                        if (_board.GameBoard[currentX][currentY] != EGameSquareState.Dark)
                        {
                            break;
                        }
                        
                        
                        //Memorizing the dark square, where we are at right now
                        alreadyBeenOnThisSquare.Add((currentX,currentY));
                        
                        
                        // if we found a piece to take   =>   we need to find additional directions after enemy piece
                        if (IsMovePossibleForKing(currentX,currentY,x2,y2,nextMoveByBlack,
                                checkersOption,calledFromController,false,
                                xStep,yStep, alreadyBeenOnThisSquare))
                        {
                            //if function returned true,
                            //we check whether the function was called from controller
                            //and whether King jumped in different diagonals (if == -1  =>  King jumped only in 1 diagonal)
                            if (calledFromController && enemyPieceX != -1 && enemyPieceY != -1)
                            {
                                ChangeSquareState(enemyPieceX,enemyPieceY, EGameSquareState.Dark);
                            }

                            //Increasing this value, knowing that move is possible and King actually took enemy piece
                            if (enemyPieceX != -1 && enemyPieceY != -1)
                            {
                                _amountOfEnemyPiecesTaken++;
                            }
                            //return true to a higher level(recursive call)
                            return true;
                        }

                    }


                    //WE ARE ON DARK SQUARE RIGHT NOW
                    if (_board.GameBoard[currentX][currentY] == EGameSquareState.Dark)
                    {
                        
                        //If it is FIRST Kings' "time" (simple short move or simple diagonal move) or when we found enemy piece in previous step
                        if (firstTime || enemyPieceFound)
                        {
                            //Memorizing the dark square, where we are at right now
                            alreadyBeenOnThisSquare.Add((currentX,currentY));
                        
                        
                            //IF TRUE => WE FOUND OUR DESTINATION
                            if (currentX == x2 && currentY == y2)
                            {
                                //IF TRUE => WE TAKE A PIECE
                                    //WE ARE EITHER DEEPER IN RECURSIVE CALL(enemyPieceFound)
                                    //OR LAST STEP IN RECURSIVE CALL(IF IT IS LAST STEP FROM RECURSION PERSPECTIVE)
                                //IF FALSE => JUST A SIMPLE MOVE WITHOUT TAKING (firstTime)
                                if (calledFromController && enemyPieceX != -1 && enemyPieceY != -1)
                                {
                                    ChangeSquareState(enemyPieceX,enemyPieceY, EGameSquareState.Dark);
                                }

                                //Increasing this value, knowing that we this is our destination, move is possible and King actually took enemy piece
                                if (enemyPieceX != -1 && enemyPieceY != -1)
                                {
                                    _amountOfEnemyPiecesTaken++;
                                }
                                return true;
                            }

                            //IF NO FLYING KINGS:   WE SHOULD NOT MOVE IN THE SAME DIRECTION FURTHER (CAN'T DO LONG JUMPS)
                            // (MEANING: IF THIS DARK SQUARE WASN'T OUR DESTINATION AND (IT IS OUR FIRST TIME OR WE MET ENEMY PIECE IN PREV STEP))
                            if (!checkersOption.FlyingKings)
                            {
                                break;
                            }
                        }
                        
                        //IF NO FLYING KINGS:   WE ARE AT DARK SQUARE AND IT IS NOT FIRST (SIMPLE MOVE) AND WE HAVE NOT MET ENEMY PIECE
                        if (!checkersOption.FlyingKings && !firstTime && !enemyPieceFound)
                        {
                            break;
                        }
                    }

                    
                    if (nextMoveByBlack)
                    {
                        if (_board.GameBoard[currentX][currentY] == EGameSquareState.WhitePiece ||
                            _board.GameBoard[currentX][currentY] == EGameSquareState.WhiteKing)
                        {
                            //saving enemy piece coordinates in case it will be taken during the move
                            enemyPieceX = currentX;
                            enemyPieceY = currentY;
                            enemyPieceFound = true;
                        }

                        //King can't jump over own pieces
                        if (_board.GameBoard[currentX][currentY] == EGameSquareState.BlackPiece ||
                            _board.GameBoard[currentX][currentY] == EGameSquareState.BlackKing)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (_board.GameBoard[currentX][currentY] == EGameSquareState.BlackPiece ||
                            _board.GameBoard[currentX][currentY] == EGameSquareState.BlackKing)
                        {
                            //saving enemy piece coordinates in case it will be taken during the move
                            enemyPieceX = currentX;
                            enemyPieceY = currentY;
                            enemyPieceFound = true;
                        }

                        //King can't jump over own pieces
                        if (_board.GameBoard[currentX][currentY] == EGameSquareState.WhitePiece ||
                            _board.GameBoard[currentX][currentY] == EGameSquareState.WhiteKing)
                        {
                            
                            break;
                        }
                    }
                    
                } while (true);
            }
        }

        return false;
    }


    private bool isMovePossibleWhenOnlyMandatoryMovesAllowed(int x1, int y1, int x2, int y2,  bool nextMoveByBlack,
        CheckersOption checkersOption, bool calledFromController, List<(int x, int y)> alreadyBeenOnThisSquare)
    {
        var boardWidth = _board.GameBoard[0].GetLength(0);
        var boardHeight = _board.GameBoard.GetLength(0);

        //List of pieces for player who is supposed to "move"
        var listOfPieces = new List<(int x, int y)>();
        
        for (int x = 0; x < boardHeight; x++)
        {
            for (int y = 0; y < boardWidth; y++)
            {
                
                if (FirstClickIsValid(x,y,nextMoveByBlack))
                {
                    //adding to the list all pieces for player who is supposed to "move"
                    listOfPieces.Add((x,y));
                }
            }
        }
        
        //List for "Amount of taken enemy pieces" for every possible move in this round for a particular player
        var listAmountOfTakenEnemyPieces = new List<int>();

        
        //NOW WE SHOULD FIND ALL POSSIBLE MOVES FOR EVERY PIECE ON THE BOARD
        //Firstly we iterate over every possible DARK square 
        for (int x = 0; x <boardHeight; x++)
        {
            for (int y = 0; y < boardWidth; y++)
            {

                
                //Now we iterate over every possible piece
                foreach (var piece in listOfPieces)
                {
                    
                    //can only move to dark square
                    if (_board.GameBoard[x][y] != EGameSquareState.Dark)
                    {
                        break;
                    } 
                    
                    //if moving a king
                    if (_board.GameBoard[piece.x][piece.y] == EGameSquareState.BlackKing || _board.GameBoard[piece.x][piece.y] == EGameSquareState.WhiteKing)
                    {
                        if (IsMovePossibleForKing(piece.x, piece.y, x, y, nextMoveByBlack,checkersOption,false,
                                true,0,0, alreadyBeenOnThisSquare))
                        {
                            //Knowing that move was successful => Adding amount of taken enemy pieces for this move
                            listAmountOfTakenEnemyPieces.Add(_amountOfEnemyPiecesTaken);
                            
                            //We should reset this variable, before starting checking another 2 coordinates
                            _amountOfEnemyPiecesTaken = 0;
                        }
                    }
                    //if moving a piece
                    else
                    {
                        if (IsMovePossibleForPiece(piece.x, piece.y, x, y, nextMoveByBlack,checkersOption,false,
                                true,0,0, alreadyBeenOnThisSquare))
                        {
                            //Knowing that move was successful => Adding amount of taken enemy pieces for this move
                            listAmountOfTakenEnemyPieces.Add(_amountOfEnemyPiecesTaken);
                            
                            //We should reset this variable, before starting checking another 2 coordinates
                            _amountOfEnemyPiecesTaken = 0;
                        }
                    }
                    
                    //we should remove all squares from this list, in order to avoid wrong calculations
                    alreadyBeenOnThisSquare.Clear();
                }
            }
        }

        //IF true => no pieces left with possible moves (For game Over function)
        if (!listAmountOfTakenEnemyPieces.Any())
        {
            return false;
        }
        
        //Calculation maximum number of possible enemy pieces to take
        var maxNumberOTakenEnemyPieces = listAmountOfTakenEnemyPieces.Max();
        
        var pieceToMove = _board.GameBoard[x1][y1];
        bool isKing = !(pieceToMove == EGameSquareState.BlackPiece || pieceToMove == EGameSquareState.WhitePiece);

        
        if (isKing)
        {
            //NOW WE CHECK IF WE CAN MAKE MOVE THAT WE ASKED ABOUT WITHOUT CHECKING IF IT IS A BEST MOVE (IF KING)
            if (IsMovePossibleForKing(x1, y1, x2, y2, nextMoveByBlack,checkersOption,false,
                    true,0,0, alreadyBeenOnThisSquare))
            {
                //we should remove all squares from this list, in order to avoid wrong calculations
                alreadyBeenOnThisSquare.Clear();
                
                //NOW WE CHECK IF IT IS A BEST MOVE TO TAKE
                if (_amountOfEnemyPiecesTaken == maxNumberOTakenEnemyPieces)
                {
                    //IF IT IS INDEED POSSIBLE AND BEST MOVE TO TAKE => WE CALL FUNCTION AGAIN WITH ANOTHER PARAMETER (calledFromController)
                    IsMovePossibleForKing(x1, y1, x2, y2, nextMoveByBlack, checkersOption, calledFromController,
                        true, 0, 0, alreadyBeenOnThisSquare);
                    //We should reset this variable, because we don't know if it is last time that we execute "isMovePossibleWhenOnlyMandatoryMovesAllowed" function
                    _amountOfEnemyPiecesTaken = 0;
                    return true;
                }
                //We should reset this variable, because we don't know if it is last time that we execute "isMovePossibleWhenOnlyMandatoryMovesAllowed" function
                _amountOfEnemyPiecesTaken = 0;
            }
        }
        else
        {
            //NOW WE CHECK IF WE CAN MAKE MOVE THAT WE ASKED ABOUT WITHOUT CHECKING IF IT IS A BEST MOVE (IF ORDINARY PIECE)
            if (IsMovePossibleForPiece(x1, y1, x2, y2, nextMoveByBlack,checkersOption,false,
                    true,0,0, alreadyBeenOnThisSquare))
            {
                //we should remove all squares from this list, in order to avoid wrong calculations
                alreadyBeenOnThisSquare.Clear();
                
                //NOW WE CHECK IF IT IS A BEST MOVE TO TAKE
                if (_amountOfEnemyPiecesTaken == maxNumberOTakenEnemyPieces)
                {

                    //IF IT IS INDEED POSSIBLE AND BEST MOVE TO TAKE => WE CALL FUNCTION AGAIN WITH ANOTHER PARAMETER (calledFromController)
                    IsMovePossibleForPiece(x1, y1, x2, y2, nextMoveByBlack, checkersOption, calledFromController,
                        true, 0, 0, alreadyBeenOnThisSquare);
                    
                    //We should reset this variable, because we don't know if it is last time that we execute "isMovePossibleWhenOnlyMandatoryMovesAllowed" function
                    _amountOfEnemyPiecesTaken = 0;
                    return true;
                }
                //We should reset this variable, because we don't know if it is last time that we execute "isMovePossibleWhenOnlyMandatoryMovesAllowed" function
                _amountOfEnemyPiecesTaken = 0;
            }
        }


        return false;
    }
    
    private void ChangeSquareState(int x, int y, EGameSquareState stateToChange)
    {
        _board.GameBoard[x][y] = stateToChange;
    }

    
    
    public bool FirstClickIsValid(int x1, int y1, bool nextMoveByBlack)
    {
        var boardWidth = _board.GameBoard[0].GetLength(0);
        var boardHeight = _board.GameBoard.GetLength(0);
        
        if (x1 < 0 || y1 < 0 || x1 >= boardHeight || y1 >= boardWidth)
        {
            return false;//incorrect input from user
        } 
        var pieceToMove = _board.GameBoard[x1][y1];

        
        if (nextMoveByBlack)
        {
            //IF IT IS BLACK MOVE => YOU CAN ONLY MOVE BLACK PIECE
            if (pieceToMove == EGameSquareState.BlackPiece || pieceToMove == EGameSquareState.BlackKing)
            {
                return true;
            }
        }
        if (nextMoveByBlack == false)
        {
            //IF IT IS WHITE MOVE => YOU CAN ONLY MOVE WHITE PIECE
            if (pieceToMove == EGameSquareState.WhitePiece || pieceToMove == EGameSquareState.WhiteKing)
            {
                return true;
            }
        }

        return false;
    }


    public int GameOver(bool nextMoveByBlack, CheckersOption checkersOption)
    {
        
        var listOfPieces = new List<(int x, int y)>();
        
        for (int x = 0; x < _board.GameBoard.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GameBoard[0].GetLength(0); y++)
            {
                
                if (FirstClickIsValid(x,y,nextMoveByBlack))
                {
                    //adding to the list all pieces for player who is supposed to "move"
                    listOfPieces.Add((x,y));
                }
            }
        }

        var gameLostByPlayer = true;
        var result = 0;

        
        //No pieces left (for a particular player) who is supposed to "move"
        if (listOfPieces.Count == 0)
        {
            if (nextMoveByBlack)
            {
                return 2; // PLAYER WITH BLACK PIECES LOST
            }

            return 1; // PLAYER WITH WHITE PIECES LOST
        }
        

        for (int x = 0; x < _board.GameBoard.GetLength(0); x++)
        {
            //just for faster calculation
            if (gameLostByPlayer == false)
            {
                //means that we already found possible move for a player => no need to continue
                break;
            }
            for (int y = 0; y < _board.GameBoard[0].GetLength(0); y++)
            {
                //just for faster calculation
                if (gameLostByPlayer == false)
                {
                    //means that we already found possible move for a player => no need to continue
                    break;
                }
                
                foreach (var piece in listOfPieces)
                {

                    //CHECKING IF PLAYER WHO IS SUPPOSED TO "MOVE" STILL HAS POSSIBLE MOVES
                    if (IsMovePossible(piece.x,piece.y,x,y,nextMoveByBlack,checkersOption,false))
                    {
                        // PLAYER WHO IS SUPPOSED TO "MOVE" STILL HAS MOVES
                        gameLostByPlayer = false;
                        break;
                    }
                    

                }
            }
        }

        if (gameLostByPlayer)
        {
            if (nextMoveByBlack)
            {
                result = 2; //PLAYER WITH BLACK PIECES LOST
            }

            if (nextMoveByBlack == false)
            {
                result = 1; //PLAYER WITH WHITE PIECES LOST
            }
            
        }

        return result;
    }

    public int CountPiecesLeftOnBoard(bool colour)
    {
        int piecesLeft = 0;

        for (int x = 0; x < _board.GameBoard.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GameBoard[0].GetLength(0); y++)
            {
                // (true) - count for "black"
                // (false) - count for "white"
                if (colour)
                {
                    if (_board.GameBoard[x][y] == EGameSquareState.BlackKing ||
                        _board.GameBoard[x][y] == EGameSquareState.BlackPiece)
                    {
                        piecesLeft++;
                    }
                }
                else
                {
                    if (_board.GameBoard[x][y] == EGameSquareState.WhiteKing ||
                        _board.GameBoard[x][y] == EGameSquareState.WhitePiece)
                    {
                        piecesLeft++;
                    }
                }
            }
        }

        return piecesLeft;
    }

}