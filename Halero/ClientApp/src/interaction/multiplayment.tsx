import { FOCUSABLE_SELECTOR } from '@testing-library/user-event/dist/utils';
import { timingSafeEqual } from 'crypto';
import React from 'react';
import { Id, toast } from 'react-toastify';
import { getCookie, removeCookie, setCookie } from 'typescript-cookie';
import { GameSessionData, UserSessionData } from '../models/game-initiaion-model';
import { GameState } from '../models/game-state-model';
import { GameUpdate } from '../models/game-update-model';
import { SnakeGlobalConfig } from '../models/snake-config-model';
import { IGamePlayer } from './game-player-interface';
import { MainPlayer } from './main-game-player';
import { OpponentPlayer } from './opponent-game-player';

type MultiPlaymentState = {
    gameConfig: SnakeGlobalConfig,
    playerScore: number,
    opponentScore: number,
    gameState: GameState,
    isInitiator: boolean
}

export class MultiplayerPlayment extends React.Component<{}, MultiPlaymentState>{
    private canvas: React.RefObject<HTMLCanvasElement>;
    private player: IGamePlayer | null = null;
    private opponent: IGamePlayer | null = null;
    private sessionID: string;
    private gameSyncSession: WebSocket | undefined = undefined;
    private pauseToast: Id | null = null;

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
            gameState: GameState.NotStarted,
            isInitiator: false
        };

        this.canvas = React.createRef();
        this.sessionID = "";

        if(getCookie("gameSessionID") !== undefined && this.sessionID === ""){
            this.sessionID = getCookie("gameSessionID")!;
            setTimeout( () => {
                console.log(getCookie("gameSessionID"));
                removeCookie("gameSessionID");
                this.startGame();
            }, 500 );
        }
        
    }

    fetchServerUpdate( updateData: MessageEvent<string> ){
        if(updateData.data === "")
            return;
        
        let updateDataString = updateData.data.replace("\u0000", "");
        console.log(JSON.parse(updateDataString), updateDataString.includes("candies"));

        if(this.state.gameState === GameState.NotStarted || (this.state.gameState === GameState.Waiting && this.player === null)){
            if(this.state.gameState === GameState.NotStarted && !updateDataString.includes("FirstUser")){
                let message: GameUpdate = JSON.parse(updateDataString);
                this.setState( { gameState: GameState.Waiting, isInitiator: true } );
                this.sessionID = message.ID;

                // ability to rejoin to dat session
                setCookie("gameSessionID", this.sessionID);
                
                toast.info("Waiting for another user to show up :3");
            }else{
                let message: GameSessionData = JSON.parse(updateDataString);
                if(!this.state.isInitiator){
                    this.sessionID = message.ID;

                    this.player = new MainPlayer(
                        this.state.gameConfig,
                        this.canvas.current!.getContext("2d")!,
                        message.SecondUser.Tail,
                        message.SecondUser.Direction,
                        this.gameSyncSession,
                        this.sessionID
                    );
                    
                    this.opponent = new OpponentPlayer(
                        this.state.gameConfig,
                        this.canvas.current!.getContext("2d")!,
                        message.FirstUser.Tail,
                        message.FirstUser.Direction
                    );

                } else {
                    this.sessionID = message.ID;

                    this.player = new MainPlayer(
                        this.state.gameConfig,
                        this.canvas.current!.getContext("2d")!,
                        message.FirstUser.Tail,
                        message.FirstUser.Direction,
                        this.gameSyncSession,
                        this.sessionID
                    );
                    
                    this.opponent = new OpponentPlayer(
                        this.state.gameConfig,
                        this.canvas.current!.getContext("2d")!,
                        message.SecondUser.Tail,
                        message.SecondUser.Direction
                    );

                }
            
                toast.info("starting");
                let candies = message.candies;
                
                console.log(candies);
                
                // this.player.

                this.player!.updateData( candies, this.opponent!);
                this.player.onScoreChange = newScore => this.setState({ playerScore: newScore });
                this.player.onGameStateChange = newState => this.setState( { gameState: newState } );

                this.opponent!.updateData( candies, undefined );

                this.player!.startGame();
                this.opponent!.startGame();
                this.setState({ gameState: GameState.Running });
                
                // ability to restore sesson
                setCookie("gameSessionID", this.sessionID);
            }
            return;
        }

        let message: GameUpdate = JSON.parse(updateDataString);

        if(message.gamePause === true && this.player!.gameState == GameState.Running){
            this.player!.pauseGame();
            this.opponent!.pauseGame();
            this.setState({ gameState: GameState.Waiting });
            this.pauseToast = toast.loading("Waiting for another player to rejoin");
            return;
        }else if(message.gamePause === false &&this.player!.gameState != GameState.Running){
            this.setState( { gameState: GameState.Running } );
            this.player!.startGame();
            this.opponent!.startGame();
            if(this.pauseToast !== null)
                toast.update(this.pauseToast, { render: "Yeey, you are back connected", isLoading: false, type: 'success' });
        }

        if(this.sessionID !== null){
            if(message.direction !== undefined && message.direction !== null){
                this.opponent!.setNewPosition(message.tail, message.direction);
                this.player!.updateData(undefined, undefined);
            }
            if(message.score !== undefined && message.score !== null){
                this.opponent!.score = message.score;
                this.setState({ opponentScore: message.score });
            }
            if(message.candies !== undefined && message.candies !== null){
                this.player!.updateData( message.candies, this.opponent! );
            }
        }

    }

    startGame(){

        if(this.gameSyncSession !== undefined)
            return;

        window.addEventListener("beforeunload", e =>{
            this.gameSyncSession!.close();
            }, false);

        this.gameSyncSession = new WebSocket(`ws${ window.location.protocol === "https:" ? 's' : '' }:${window.location.host}/api/SnakeManager/getUpdate`);
        

        this.gameSyncSession!.onclose = (e) => { 
            alert("Last game communication stream has been closed");
            removeCookie("gameSessionID");
        };
        this.gameSyncSession!.onmessage = (e) => this.fetchServerUpdate(e);
        this.gameSyncSession!.onopen = (e) => {
            this.gameSyncSession!.send( JSON.stringify({
                ID: this.sessionID == "" ? null : this.sessionID,
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
                    this.state.gameState !== GameState.NotStarted ?
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
                    this.state.gameState !== GameState.NotStarted ?
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
                    this.state.gameState === GameState.NotStarted ?
                            <div className="introduction">
                                <button onClick={() => this.startGame()} className="startOnlineGameBtn">Find opponent</button>
                                <button onClick={() => this.getControllsInfo()} className="controllsGameBtn">Controlls</button>
                            </div> 
                        : null
                }
            </div>
            </>
        );
    }
}