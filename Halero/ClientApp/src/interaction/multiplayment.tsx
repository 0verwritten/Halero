import React from 'react';
import { toast } from 'react-toastify';
import cookies, { getCookie, removeCookie, setCookie } from 'typescript-cookie';
import { GameState } from '../models/game-state-model';
import { SnakeGlobalConfig } from '../models/snake-config-model';
import { IGamePlayer } from './game-player-interface';
import { MainPlayer } from './main-game-player';
import { OpponentPlayer } from './opponent-game-player';

type MultiPlaymentState = {
    gameConfig: SnakeGlobalConfig,
    playerScore: number,
    opponentScore: number,
    gameState: GameState
}

export class MultiplayerPlayment extends React.Component<{}, MultiPlaymentState>{
    private canvas: React.RefObject<HTMLCanvasElement>;
    private player: IGamePlayer | null = null;
    private opponent: IGamePlayer | null = null;
    private sessionID: string;
    private gameSyncSession: WebSocket | undefined = undefined;

    constructor( props: {} ){
        super(props);
        this.state = {
            gameConfig: {
                speed: 1,
                size: 10,
                width: 300,
                height: 300,
                candyColor: "#FF8888",
                frameRate: 1000 / 60 // 60 fps
            },
            playerScore: 0,
            opponentScore: 0,
            gameState: GameState.NotStarted
        };

        this.canvas = React.createRef();
        this.sessionID = "";

        if(getCookie("gameSessionID") != undefined){
            this.sessionID = getCookie("gameSessionID")!;
            setTimeout( () => {
                console.log(getCookie("gameSessionID"));
                removeCookie("gameSessionID");
                this.startGame();
            }, 1000 );
        }
        
    }

    // // pauseForAnotherPlayer(setOnPause){
    // //     if(this.state.gamePause == null || setOnPause == true){
    // //         this.setState({ gamePause: toast.loading("The connection has been lost"), gameOver: true }); 
    // //         clearInterval(this.state.playerSnake.gameUpdate);      
    // //         clearInterval(this.state.oppontSnake.gameUpdate);

    // //         this.state.gameSync.send(JSON.stringify({
    // //             ID: this.state.sessionID,
    // //             tail: this.state.playerSnake.tail
    // //         }));
    // //     }
    // //     else{
    // //         toast.success(this.state.gamePause, { render: "All is good", type: "success", isLoading: false });
            
    // //         let player = this.state.playerSnake;
    // //         let opponent = this.state.oppontSnake;
    // //         player.gameUpdate = setInterval( () => this.updateFrame(), this.state.updateInterval );
    // //         opponent.gameUpdate = setInterval( () => this.updateFrame(true), this.state.updateInterval );

    // //         this.setState({
    // //             playerSnake: player, 
    // //             oppontSnake: opponent
    // //         });
    // //     }
    // // }

    fetchServerUpdate( updateData: MessageEvent<string> ){
        if(updateData.data == "")
            return;
        
        let message = JSON.parse(updateData.data.replace("\u0000", ""));
        console.log(message);

        if(this.state.gameState == GameState.NotStarted){
            if(message.FirstUser != undefined && this.sessionID == null){
                this.player = new MainPlayer(
                    this.state.gameConfig,
                    this.canvas.current!.getContext("2d")!,
                    message.SecondUser.Tail,
                    message.SecondUser.Direction
                );
                // player.direction = message.FirstUser.Direction;
                // player.tail = message.FirstUser.Tail;
                this.opponent = new OpponentPlayer(
                    this.state.gameConfig,
                    this.canvas.current!.getContext("2d")!,
                    message.FirstUser.Tail,
                    message.FirstUser.Direction
                );
                // player.direction = message.SecondUser.Direction;
                // player.tail = message.SecondUser.Tail;
                // opponent.direction = message.FirstUser.Direction;
                // opponent.tail = message.FirstUser.Tail; 

                this.sessionID = message.ID;
            }
            else if(message.FirstUser != undefined && this.state.gameState == GameState.NotStarted){
                this.player = new MainPlayer(
                    this.state.gameConfig,
                    this.canvas.current!.getContext("2d")!,
                    message.FirstUser.Tail,
                    message.FirstUser.Direction
                );
                // player.direction = message.FirstUser.Direction;
                // player.tail = message.FirstUser.Tail;
                this.opponent = new OpponentPlayer(
                    this.state.gameConfig,
                    this.canvas.current!.getContext("2d")!,
                    message.SecondUser.Tail,
                    message.SecondUser.Direction
                );
                // opponent.direction = message.SecondUser.Direction;
                // opponent.tail = message.SecondUser.Tail;
            }
            else if(this.sessionID == null && message.gameOver != true){
                this.setState( { gameState: GameState.Waiting } );
                toast.info("Waiting for another user to show up :3");
            }
            
            if(this.sessionID != null){
                toast.info("starting");
                let candies = message.candies;
                
                console.log(candies);
                this.player!.updateData( candies, this.opponent!);
                this.opponent!.updateData( candies, undefined );

                // player.gameUpdate = setInterval( () => this.updateFrame(), this.state.updateInterval );
                // opponent.gameUpdate = setInterval( () => this.updateFrame(true), this.state.updateInterval );
                
                // ability to restore sesson
                setCookie("gameSessionID", this.sessionID);

            }
            return;
        }

        // if(message.gamePause == true){
        //     this.pauseForAnotherPlayer(true);
        //     return;
        // } 
        // else if(this.state.gamePause != null && message.gamePause == false){
        //     this.pauseForAnotherPlayer(false);
        // }
        
        // if(sessionID != null){
        //     let opponent = this.state.oppontSnake;
        //     if(message.direction != undefined && message.direction != null){
        //         opponent.direction = message.direction;
        //         opponent.tail = message.tail;
        //     }
        //     if(message.score != undefined && message.score != null){
        //         opponent.score = message.score;
        //     }
        //     this.setState({ oppontSnake: opponent });
        //     if(message.candies != undefined && message.candies != null){
        //         this.setState({ candies: message.candies });
        //     }
        //     this.refreshCanvas();
        // }

    }

    startGame(){

        this.gameSyncSession = new WebSocket(`ws${ window.location.protocol == "https:" ? 's' : '' }:${window.location.host}/api/SnakeManager/getUpdate`);
        

        this.gameSyncSession!.onclose = (e) => { 
            alert("Last game communication stream has been closed");
            removeCookie("gameSessionID", { path: "" });
        };
        this.gameSyncSession!.onmessage = (e) => this.fetchServerUpdate(e);
        this.gameSyncSession!.onopen = (e) => {
            this.gameSyncSession!.send( JSON.stringify({
                gameOver: false
            }));
        };
    }
    getControllsInfo(){
        toast.info("To rotate the snake use:  ← ↑ → ↓ ")
        toast.info("To set pause use:         p");
    }
    
    render(){
        require('./css/game.css');
        return (
            <>
            <div className="gamePlate singleUser">
                
                {
                    this.state.gameState != GameState.NotStarted ?
                    <div className="userPlate yours">
                        <div className="profileName">
                            You
                        </div>
                        <div className="score">
                            Score: <span>{ this.state.playerScore }</span>
                        </div>
                    </div>
                    : null
                }
                <canvas className="gamePlayCanvas" width="300" height="300" ref={this.canvas}/>
                {
                    this.state.gameState != GameState.NotStarted ?
                    <div className="userPlate opponent hidden">
                        <div className="profileName">
                            Enemy
                        </div>
                        <div className="score">
                            Score: <span>{ this.state.opponentScore }</span>
                        </div>
                    </div>
                    : null
                }
                { 
                    this.state.gameState == GameState.NotStarted ?
                            <div className="introduction">
                                <button onClick={() => this.startGame()} className="startOnlineGameBtn">Find opponent</button>
                                <button onClick={() => this.getControllsInfo()} className="controllsGameBtn">Controlls</button>
                            </div> 
                        : null
                }
            </div>
            <button onClick={ () => removeCookie("gameSessionID", { path: "" }) }>end</button>
            </>
        );
    }
}