import { Link } from "react-router-dom";

function Navbar() {
    return (
        <nav>
            <Link to="/register">Register</Link> |{" "}
            <Link to="/login">Login</Link> |{" "}
            <Link to="/products">Products</Link> |{" "}
        </nav>
    );
}

export default Navbar;