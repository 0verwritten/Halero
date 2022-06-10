import React from 'react';
import "./css/layout.css";
import { removeCookie } from "typescript-cookie";
import image from "./imgs/arrow.svg";
import { Link } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';

type SnakeLayputProps = {
    children: (string | React.ReactElement)[];
}

export class SnakeLayout extends React.Component< SnakeLayputProps >{
    private nav: React.RefObject<HTMLDivElement> = React.createRef();
    private navClosingTimer: NodeJS.Timeout | null = null;
    private navState = false;

    constructor(props: SnakeLayputProps){
        super(props);
        this.state = {
            navState: false,
        }
    }

    toggleNavbar(state = !this.navState){
        if(state)
            this.nav.current!.classList.add('active');
        else
            this.nav.current!.classList.remove('active');
        this.navState = state;
    }

    render(){
        return (
            <>
            <ToastContainer
                position="top-right"
                autoClose={5000}
                newestOnTop={false}
                closeOnClick
                rtl={false}
                pauseOnFocusLoss
                draggable
                pauseOnHover
            />
            
            <nav className="navibar" ref={this.nav} 
                    onMouseLeave={ () => this.navClosingTimer = setTimeout( () => this.toggleNavbar( false ), 500)}
                    onMouseEnter={ () => { if(this.navClosingTimer != null) clearTimeout(this.navClosingTimer); } }>
                <div className="linkList">
                    <Link to="/profile" className="hypernav" >Profile</Link>
                    <Link to="/solo" className="hypernav">Play solo</Link>
                    <Link to="/online/play" className="hypernav">Play multiplayer</Link>
                    <Link to="/online/active" className="hypernav">Active games</Link>
                    <button className="hypernav"
                        onClick={ () => {
                            removeCookie("accessToken", { path: "" });
                            removeCookie("refreshToken", { path: "" });
                            window.location.reload();
                        }}>Log out</button>
                </div>
                <button className="slider" onClick={ e => this.toggleNavbar() }>
                    <img src={ image } alt="/\" />
                </button>
            </nav>
            {
                this.props.children.map( v => v )
            }
            </>
        );
    }
}