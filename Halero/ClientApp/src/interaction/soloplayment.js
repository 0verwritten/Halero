import React from 'react';
import { toast } from 'react-toastify';


export class SoloPlayment extends React.Component{
    constructor(props){
        super(props);

        this.state = {
            tail: [ [150, 150] ],
            candies: [],
            direction: 0, // 0 - right, 1 - left, 2 - top, 3 - down
            speed: 2,
            size: 10,
            extend: 0,
            width: 300,
            height: 300,
            score: 0,
            gameUpdate: null,
        };

        this.canvas = React.createRef();
        this.changeDirection = this.changeDirection.bind( this );
    }
    
    updateFrame(){
        
        let horizontal  = this.state.direction < 2 ? this.state.direction       * 2 - 1 : 0;
        let vertical    = this.state.direction >= 2 ? (this.state.direction % 2) * 2 - 1 : 0;
        let speed       = this.state.speed;
        let tail        = this.state.tail;


        if(this.canvas.current == null)
            return;

        let field       = this.canvas.current.getContext('2d');

        


        let x = tail[0][0] + (speed) * horizontal,
            y = tail[0][1] + (speed) * vertical;

        if( x > 300 )   x = 0;
        if( x < 0 )     x = 290;
        if( y > 300 )   y = 0;
        if( y < 0 )     y = 290;

        
        let candies     = this.cheackForCandies(x,y, field);
        if(candies.length === 0){
            candies = this.getNewCandies(candies, field)
        }
        field.fillStyle = 'rgb(200, 0, 0)';
        field.fillRect( x, y, this.state.size, this.state.size );

        // if(Math.abs(tail[0][0] - x) >= 10 || Math.abs(tail[0][1] - y) >= 10){
            if(this.state.extend === 0){
                field.clearRect( tail[tail.length - 1][0], tail[tail.length - 1][1], this.state.size, this.state.size );
                tail.pop();     
            } 
            else{
                this.state.extend--;
            }

            
            tail.unshift([x, y]);
            // this.state.addance = [0,0];
            for(let i = 0; i < tail.length; i++){
                field.fillRect( tail[i][0], tail[i][1], this.state.size, this.state.size );
            }
        // }
        

        this.setState({ tail: tail, candies: candies });
        // this.setState({ tail: tail, addance: [ (this.state.addance[0] + 1) * horizontal, (this.state.addance[1] + 1) * vertical ] });
    }

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
                this.setState({extend: 5, score: this.state.score + 1} )
            }
        }
        candies = candies.concat(newCandies);
        return candies;
    }

    checkTail( x, y, size = this.state.size){
        return 
    }

    getNewCandies(candies, field){
        let circleR = this.state.size / 3;
        let x, y;
        do{
        x = Math.floor(Math.random() * (this.state.width  - 2 * circleR) + circleR);
        y = Math.floor(Math.random() * (this.state.height - 2 * circleR) + circleR);
        }while(!this.state.tail.every( piece => {
                    return  !this.checkInterception(piece, [x,y]);
                }));
        candies.push([ x, y ]);
        
        field.fillStyle = "#000000";
        field.beginPath();
        field.arc(x + circleR, y + circleR, circleR, 0, 2 * Math.PI, false);
        field.fill();
        return candies
    }

    changeDirection(e){
        let direction = this.state.direction;
        let newDirection = direction;

        if(e.keyCode == 80){
            if(this.state.gameUpdate != null){
                clearInterval(this.state.gameUpdate);
                this.setState( { gameUpdate: null } );
                toast.info("Game is on pause", { autoClose: 3000 });
            }else{
                this.setState( { gameUpdate: setInterval( () => this.updateFrame(), 1000/60 ) } );
            }
        }
        if(this.state.gameUpdate == null)
            return;

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
        if(e.keyCode == 32){
            this.setState({extend: 5} )
        }
        if(direction != newDirection)
            this.setState( { direction: newDirection } );
    }

    startGame(e){
        this.setState({ gameUpdate: setInterval( () => this.updateFrame(), 1000/60 ) });
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
                <div className="userPlate yours">
                    <div className="score">
                        Score: <span>{ this.state.score }</span>
                    </div>
                </div>
                <canvas className="gamePlayCanvas solo" width="300" height="300" ref={this.canvas}/>
            </div>
            { 
                this.state.candies.length == 0 ?
                        <div className="introduction">
                            <button onClick={e => this.startGame(e)} className="startGameBtn">Start Game</button>
                            <button onClick={() => this.getControllsInfo()} className="ControllsGameBtn">Controlls</button>
                        </div> 
                    :
                        null
            }
            </>
        );
    }
}