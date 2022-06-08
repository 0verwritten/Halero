import React from 'react';
import { toast } from 'react-toastify';

export class MultiplayerPlayment extends React.Component{
    constructor( props ){
        super(props);
        this.state = {
            playerSnake: {
                tail: [ [150, 150] ],
                direction: 0, // 0 - right, 1 - left, 2 - top, 3 - down
                extend: 0,
                snakeColor: "#C80000",
                score: 0,
                gameUpdate: null
            },
            oppontSnake:{
                tail: [],
                direction: 0,
                extend: 0,
                snakeColor: "#C80000",
                score:0,
                gameUpdate: null
            },
            candies: [],
            speed: 2,
            updateInterval: 1000/30,
            size: 10,
            width: 300,
            height: 300,
            foodColor: "#FF8888",
            gameSync: null,
            // gameSync: new WebSocket(),
            isSecondUser: true,
            sessionID: null
        };

        this.canvas = React.createRef();
        this.changeDirection = this.changeDirection.bind( this );
    }
    
    updateFrame(isOpponent = false){
        if(this.gameOver)
            return;

        let snakeState = this.state.playerSnake;
        if(isOpponent) 
            snakeState = this.state.oppontSnake;
        
        let horizontal  = snakeState.direction < 2 ? snakeState.direction       * 2 - 1 : 0;
        let vertical    = snakeState.direction >= 2 ? (snakeState.direction % 2) * 2 - 1 : 0;
        let speed       = this.state.speed;
        let tail        = snakeState.tail;


        if(this.canvas.current == null)
            return;

        let field       = this.canvas.current.getContext('2d');

        this.refreshWindow();


        let x = tail[0][0] + (speed) * horizontal,
            y = tail[0][1] + (speed) * vertical;

        if( x > 300 )   x = 0;
        if( x < 0 )     x = 290;
        if( y > 300 )   y = 0;
        if( y < 0 )     y = 290;

        let candies = this.state.candies;
        if(!isOpponent)
            candies = this.cheackForCandies(x,y, field);

        field.fillStyle = snakeState.snakeColor;
        field.fillRect( x, y, this.state.size, this.state.size );

        // if(Math.abs(tail[0][0] - x) >= 10 || Math.abs(tail[0][1] - y) >= 10){
            if(snakeState.extend === 0){
                field.clearRect( tail[tail.length - 1][0], tail[tail.length - 1][1], this.state.size, this.state.size );
                tail.pop(); 
            } 
            else{
                this.state.gameSync.send(JSON.stringify({
                    ID: this.state.sessionID,
                    tail: tail,
                    direction: snakeState.direction
                }));
                snakeState.extend--;
            }

            
            tail.unshift([x, y]);

            // this.state.addance = [0,0];
            for(let i = 0; i < tail.length; i++){
                field.fillRect( tail[i][0], tail[i][1], this.state.size, this.state.size );
            }
        // }
        
        snakeState.tail = tail;
        this.setState({ [isOpponent ? "opponentSnake": "playerSnake"]: snakeState, candies: candies });
        // this.setState({ tail: tail, addance: [ (this.state.addance[0] + 1) * horizontal, (this.state.addance[1] + 1) * vertical ] });
    }

    // intercaption with candies
    checkInterception(piece, middleOne, size = this.state.size){
        function inBetween(a, b, c){
            return a < c && b > c;
        };

        return  inBetween(piece[0] - size/3, piece[0] + size + size/3, middleOne[0]) 
                    &&
                inBetween(piece[1] - size/3, piece[1] + size + size/3, middleOne[1]);
    }

    cheackForCandies(x, y, field){
        let candies = this.state.candies;
        let newCandies = [];
        for(let i = 0; i < candies.length; i++){
            if( this.checkInterception([x, y], candies[i] ) ){
                field.clearRect(candies[i][0], candies[i][1], this.state.size, this.state.size);
                newCandies = this.getNewCandies(newCandies, field);

                candies = candies.filter( (val) => val !== candies[i] );
                i--;
                let player = this.state.playerSnake;
                player.extend = 5;
                this.setState({playerSnake: player, score: this.state.score + 1} )
            }
        }
        candies = candies.concat(newCandies);
        return candies;
    }

    checkTail( x, y, size = this.state.size){
        return 
    }

    getNewCandies(candies, field, update = true){
        let circleR = this.state.size / 3;
        let x, y;
        do{
        x = Math.floor(Math.random() * (this.state.width  - 2 * circleR) + circleR);
        y = Math.floor(Math.random() * (this.state.height - 2 * circleR) + circleR);
        }while(!this.state.playerSnake.tail.every( piece => {
                    return  !this.checkInterception(piece, [x,y]);
                }) 
                    &&
               !this.state.oppontSnake.tail.every( piece => {
                    return  !this.checkInterception(piece, [x,y]);
                }) );
        candies.push([ x, y ]);

        if(update){
            let snake = this.state.playerSnake;
            snake.score++;
            this.setState({
                playerSnake: snake
            });
            this.state.gameSync.send( JSON.stringify({ 
                ID: this.state.sessionID,
                candies: candies,
                score: snake.score
            }) + "\0");
        }
        
        field.fillStyle = this.state.foodColor;
        field.beginPath();
        field.arc(x + circleR, y + circleR, circleR, 0, 2 * Math.PI, false);
        field.fill();
        return candies
    }

    refreshWindow(){
        let field       = this.canvas.current.getContext('2d');

        field.fillStyle = "#FFFAEE";
        field.fillRect( 0,0, this.state.width, this.state.height );

        field.fillStyle = this.state.playerSnake.snakeColor;
        for(let i = 0; i < this.state.playerSnake.tail.length; i++){
            field.fillRect( this.state.playerSnake.tail[i][0], this.state.playerSnake.tail[i][1], this.state.size, this.state.size );
        }

        field.fillStyle = this.state.oppontSnake.snakeColor;
        for(let i = 0; i < this.state.oppontSnake.tail.length; i++){
            field.fillRect( this.state.oppontSnake.tail[i][0], this.state.oppontSnake.tail[i][1], this.state.size, this.state.size );
        }

        let circleR = this.state.size / 3;
        field.fillStyle = this.state.foodColor;
        for(let i = 0;i < this.state.candies.length; i++){
            field.beginPath();
            field.arc(this.state.candies[i][0] + circleR, this.state.candies[i][1] + circleR, circleR, 0, 2 * Math.PI, false);
            field.fill();
        }

    }

    changeDirection(e){
        let direction = this.state.playerSnake.direction;
        let newDirection = direction;

        if(e.keyCode == 37 && direction != 1){
            newDirection = 0;
        }
        if(e.keyCode == 38 && direction != 3){
            newDirection = 2;
        }
        if(e.keyCode == 39 && direction != 0){
            newDirection = 1;
        }
        if(e.keyCode == 40 && direction != 2){
            newDirection = 3;
        }
        if(direction != newDirection){
            let snake = this.state.playerSnake;
            snake.direction = newDirection
            this.setState( { playerSnake : snake } );

            this.state.gameSync.send( JSON.stringify({
                ID: this.state.sessionID,
                direction: newDirection,
                tail: this.state.playerSnake.tail,
            }));
        }
    }

    fetchServerUpdate( updateData ){
        if(updateData.data == "")
            return;
        
        let message = JSON.parse(updateData.data.replace("\u0000", ""));
        let sessionID = this.state.sessionID;

        if(this.state.candies == -1){
            let player = this.state.playerSnake;
            let opponent = this.state.oppontSnake;
            if(message.FirstUser != undefined && sessionID == null){
                player.direction = message.SecondUser.Direction;
                player.tail = message.SecondUser.Tail;
                opponent.direction = message.FirstUser.Direction;
                opponent.tail = message.FirstUser.Tail; 

                sessionID = message.ID;
            }
            else if(message.FirstUser != undefined && this.state.candies == -1){
                player.direction = message.FirstUser.Direction;
                player.tail = message.FirstUser.Tail;
                opponent.direction = message.SecondUser.Direction;
                opponent.tail = message.SecondUser.Tail;
            }
            else if(sessionID == null){
                this.setState( { sessionID: message.ID, isSecondUser: false } );
                toast.info("Waiting for another user to show up)");
            }
            
            if(sessionID != null){
                let candies = message.candies;
                this.refreshWindow();
                
                console.log(candies);
                player.gameUpdate = setInterval( () => this.updateFrame(), this.state.updateInterval );
                opponent.gameUpdate = setInterval( () => this.updateFrame(true), this.state.updateInterval );

                this.setState({ 
                    candies: candies,
                    sessionID: sessionID,
                    playerSnake: player, 
                    oppontSnake: opponent
                });
                return;
            }
        }
        
        if(sessionID != null){
            let opponent = this.state.oppontSnake;
            if(message.direction != undefined){
                opponent.direction = message.direction;
                opponent.tail = message.tail;
            }
            if(message.score != undefined){
                opponent.score = message.score;
            }
            this.setState({ oppontSnake: opponent });
            if(message.candies != undefined){
                this.setState({ candies: message.candies });
            }
            this.refreshWindow();
        }

    }

    startGame(e){
        this.setState({ candies: -1 });
        let gameSyncSession = new WebSocket(`ws${ window.location.protocol == "https:" ? 's' : '' }:${window.location.host}/api/SnakeManager/getUpdate`);
        this.setState({
            gameSync: gameSyncSession
        });
        gameSyncSession.onclose = (e) => {
            this.setState({ gameOver: true }); 
            clearInterval(this.state.playerSnake.gameUpdate);      
            clearInterval(this.state.oppontSnake.gameUpdate);      
        };
        gameSyncSession.onmessage = (e) => this.fetchServerUpdate(e);
        gameSyncSession.onopen = (e) => {
            console.log(e);
            gameSyncSession.send( JSON.stringify({
                gameOver: false,
                candies: []
            }));
        };
    }
    getControllsInfo(){
        toast.info("To rotate the snake use:  ← ↑ → ↓ ")
        toast.info("To set pause use:         p");
    }
    
    render(){
        document.getElementsByTagName("body")[0].addEventListener('keydown', this.changeDirection);
        require('./css/game.css');
        return (
            <>
            <div className="gamePlate singleUser">
                
                {
                    this.state.candies.length != 0 ?
                    <div className="userPlate yours">
                        <div className="profileName">
                            You
                        </div>
                        <div className="score">
                            Score: <span>{ this.state.playerSnake.score }</span>
                        </div>
                    </div>
                    : null
                }
                <canvas className="gamePlayCanvas" width="300" height="300" ref={this.canvas}/>
                {
                    this.state.candies.length != 0 ?
                    <div className="userPlate opponent hidden">
                        <div className="profileName">
                            Enemy
                        </div>
                        <div className="score">
                            Score: <span>{ this.state.oppontSnake.score }</span>
                        </div>
                    </div>
                    : null
                }
                { 
                    this.state.candies.length == 0 ?
                            <div className="introduction">
                                <button onClick={e => this.startGame(e)} className="startOnlineGameBtn">Find opponent</button>
                                <button onClick={() => this.getControllsInfo()} className="controllsGameBtn">Controlls</button>
                            </div> 
                        : null
                }
            </div>
            </>
        );
        // return (
        //     <div className="gamePlate singleUser">
        //         <div className="userPlate yours">
        //             <div className="profileName">
        //                 Person
        //             </div>
        //             <div className="score">
        //                 Score: <span>666</span>
        //             </div>
        //         </div>
        //         <canvas className="gamePlayCanvas"></canvas>
        //         <div className="userPlate opponent hidden">
        //             <div className="profileName">
        //                 Person
        //             </div>
        //             <div className="score">
        //                 Score: <span>0</span>
        //             </div>
        //         </div>
        //     </div>
        // );
    }
}