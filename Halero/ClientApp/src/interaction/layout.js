import React from 'react';
import "./css/layout.css";
import cookies from "js-cookies";
import image from "./imgs/arrow.svg";
import { Link } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';

export class SnakeLayout extends React.Component{
    constructor(props){
        super(props);
        this.state = {
            navState: false,
        }
        this.nav = React.createRef();
        this.navClosingTimer = null;
    }

    toggleNavbar(state = !this.state.navState){
        if(state)
            this.nav.current.classList.add('active');
        else
            this.nav.current.classList.remove('active');
        this.setState({ navState: state });
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
                            cookies.removeItem("accessToken");
                            cookies.removeItem("refreshToken");
                            window.location.reload();
                        }}>Log out</button>
                </div>
                <button className="slider" onClick={ e => this.toggleNavbar() }>
                    <img src={image} alt="/\" />
                </button>
            </nav>
            {
                this.props.children.map( v => v )
            }
            </>
        );
    }
}