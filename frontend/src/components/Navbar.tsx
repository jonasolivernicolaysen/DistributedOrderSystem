import { Link } from "react-router-dom";
import { useAuth } from "../pages/AuthContext"

function Navbar() {
    // check if user is logged in by validating token
    const { isLoggedIn } = useAuth();

    return (

        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
            <div className="container">
                {isLoggedIn && (
                    <div className="navbar-nav flex-row gap-3">
                        <Link className="nav-link" to="/products">Products</Link>
                        <Link className="nav-link" to="/cart">Cart</Link>
                        <Link className="nav-link" to="/profile">Profile</Link>
                        <Link className="nav-link" to="/login">Sign Out</Link>
                    </div>
                )}

                {!isLoggedIn && (
                    <div className="navbar-nav flex-row gap-3 ms-auto">
                        <Link className="nav-link" to="/login">Login</Link>
                        <Link className="nav-link" to="/register">Register</Link>
                    </div>
                )}
            </div>
        </nav>
    );
}

export default Navbar;